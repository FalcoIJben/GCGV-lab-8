using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace lab8.Assets.Scripts
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Landscape : MonoBehaviour
    {
        private bool _isDirty;
        private Mesh _mesh;
        [SerializeField] private Gradient gradient;
        
        [Range(0, 1)] [SerializeField] private float gain = 0.5f;
        [Range(1, 3)] [SerializeField] private float lacunarity = 2f;
        [Range(1, 8)] [SerializeField] private int octaves = 4;

        [SerializeField] private float scale = 5f;
        [SerializeField] private Vector2 shift = Vector2.zero;
        [SerializeField] private int state = 0;
        [SerializeField] private int resolution = 128;
        [SerializeField] private float length = 256f;
        [SerializeField] private float height = 50f;
		private Vector3[] vertices;
		private int[] triangles;

        private void Awake()
        {
            (GetComponent<MeshFilter>().mesh = _mesh = new Mesh {name = name}).MarkDynamic(); 
	    	Update();
        }

        private void OnValidate()
        {
            _isDirty = true;
        }

        private void Update()
        {
            if (!_isDirty) return;

            GenerateLandscape();
            _isDirty = false;
        }

        private void GenerateLandscape()
        {
	    	var vertices = new Vector3[(int)((this.resolution+1)*(this.resolution+1))];
			var colors = new Color[(vertices.Length)];
	    	int index = 0;
	    	for (int z=0; z<=this.resolution; z++) 
	   		{
				for (int x=0; x<=this.resolution; x++) 
				{
				    float f = FractalNoise(new Vector2(z,x), this.gain, this.lacunarity, this.octaves, this.scale, this.shift, this.state);
					float h = IslandFilter(x, f, z) * this.height * this.gain;
				    vertices[index] = new Vector3(x,h,z);
					colors[index] = gradient.Evaluate(h/(height*gain/1.3));
				    index++;
				}
	    	}
	    	this.vertices = vertices;

	     
	    	var triangles = new int[6*resolution*resolution];
	    	index = 0; //reuse index;
	    	int y = 0;
	    	for (int z = 0; z<resolution; z++) 
	    	{
	    	    for (int x = 0; x<resolution; x++) 
	    	    { 
	    	        triangles[index+0] = y + 0;  
	    	        triangles[index+1] = y + this.resolution + 1;  
	    		    triangles[index+2] = y + 1;
	    	 	    triangles[index+3] = y + 1;
	    		    triangles[index+4] = y + this.resolution + 1;
	    		    triangles[index+5] = y + this.resolution + 2;
			
			    	index+=6;
			    	y++;
	    		}
				y++;
	    	}	
            

	    	_mesh.Clear();
            _mesh.SetVertices(new System.Collections.Generic.List<Vector3> (vertices));
            _mesh.SetColors(new System.Collections.Generic.List<Color> (colors)); 
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
        }

		private float IslandFilter(float x, float y, float z) {
			var halfSize = this.resolution / 2;
			var cx = x - halfSize;
			var cz = z - halfSize;
			var r = Math.Sqrt(cx * cx + cz * cz) / halfSize;
			
			// Fall off to zero the last quarter of the radius.
			var p = (1 - r) * 4;
			if (p < 0) {
				return 0;
			}
			else if (p >= 1) {
				return y;
			}
			else {
				return (float) (p * y);
			}
		}

        private float FractalNoise(Vector2 coords, float gain, float lacunarity, int octaves, float scale,
            Vector2 shift, int state)
        {
            /*
            * Tip:
            * Here, you can use the built-in Perlin noise implementation for each octave:
            * Mathf.PerlinNoise(x, y); such that:
            
            * x = coords.x * frequency.x * scale + some random number (seeded by state at the beginning) + shift.x; and
            * y = coords.y * frequency.y * scale + some random number (seeded by state at the beginning) + shift.y; and
            */
	    	Random.InitState(state);
            var ran = new Random();
	    	float total = 0;
			float maxValue = 0;  // Used for normalizing result
			for (int i = 0; i < octaves; i++) {
				var x = coords.x * lacunarity * scale + Random.value + shift.x;
				var y = coords.y * lacunarity * scale + Random.value + shift.y;           
				
				total += Mathf.PerlinNoise(x, y) * gain;
				lacunarity /= 2;
				maxValue += gain;

			}
			return total/maxValue;
        }

	    public void AdjustOctaves(float newOctaves) {
			Debug.Log(newOctaves);
			this.octaves = (int) newOctaves;
			GenerateLandscape();
		}

	    public void AdjustGain(float newGain) {
			this.gain = newGain;
			GenerateLandscape();
		}

	    public void AdjustLacunarity(float newLacunarity) {
			this.lacunarity = newLacunarity;
			GenerateLandscape();
		}

    }

}
