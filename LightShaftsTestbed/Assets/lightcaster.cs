using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class lightcaster : MonoBehaviour
{
    
	public GameObject[] sceneObjects; //The objects in the scene to effect the lighting.

	private Mesh mesh; //The light mesh.

    public GameObject lightRays; //the light game object.

    public float offset = 0.0001f; //the offset of the two rays cast to the left and right of each vertex of the scene objects.

    public bool showRed; //For debugging: shows the red rays casted (the negative offset rays).
    public bool showGreen; //For debugging: shows the green rays casted (the positive offset rays).

    public struct angledVerts{ //used for updating the vertices and UVs of the light mesh. The angle variable is for properly sorting the ray hit points.
        public Vector3 vert;
        public float angle;
        public Vector2 uv;
    }

	// Use this for initialization
	void Start () {
		mesh = lightRays.GetComponent<MeshFilter>().mesh; //inits the mesh of the light.
	}


    /// <summary>
    /// Adds three ints to the end of an int array.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="itemToAdd1"></param>
    /// <param name="itemToAdd2"></param>
    /// <param name="itemToAdd3"></param>
    /// <returns></returns>
	public static int[] AddItemsToArray (int[] original, int itemToAdd1, int itemToAdd2, int itemToAdd3) {
      int[] finalArray = new int[ original.Length + 3 ];
      for(int i = 0; i < original.Length; i ++ ) {
           finalArray[i] = original[i];
      }
      finalArray[original.Length] = itemToAdd1;
      finalArray[original.Length + 1] = itemToAdd2;
      finalArray[original.Length + 2] = itemToAdd3;
      return finalArray;
 	}

    /// <summary>
    /// Adds two arrays together, making a third array.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static Vector3[] ConcatArrays(Vector3[] first, Vector3[] second){
        Vector3[] concatted = new Vector3[first.Length + second.Length];

        Array.Copy(first, concatted, first.Length);
        Array.Copy(second, 0, concatted, first.Length, second.Length);

        return concatted;
     }

	// Update is called once per frame
	void Update()
    {
		mesh.Clear(); //clears the mesh before changing it.

        // The next few lines create an array to store all vertices of all the scene objects that should react to the light.
		Vector3[] objverts = sceneObjects[0].GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 1; i < sceneObjects.Length; i++)
        {
            objverts = ConcatArrays(objverts, sceneObjects[i].GetComponent<MeshFilter>().mesh.vertices);
        }
        
        //these lines (1) an array of structs which will be used to populate the light mesh and (2) the vertices and UVs to ultimately populate the mesh.
        // (the "*2" is because there are twice as many rays casted as vertices, and the "+1" because the first point in the mesh should be the center of the light source)
        angledVerts[] angleds = new angledVerts[(objverts.Length*2)];
		Vector3[] verts = new Vector3[(objverts.Length*2)+1];
        Vector2[] uvs = new Vector2[(objverts.Length*2)+1];


        //Store the vertex location and UV of the center of the light source in the first locations of verts and uvs.
		verts[0] = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
		uvs[0] = new Vector2(lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position).x, lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position).y);

        int h = 0; //a constantly increasing int to use to calculate the current location in the angleds struct array.

        for (int j = 0; j < sceneObjects.Length; j++) //cycle through all scene objects.
        {
            for (int i = 0; i < sceneObjects[j].GetComponent<MeshFilter>().mesh.vertices.Length; i++) //cycle through all vertices in the current scene object.
		    {
                Vector3 me = this.transform.position;// just to make the current position shorter to reference.
                Vector3 other = sceneObjects[j].transform.localToWorldMatrix.MultiplyPoint3x4(objverts[h]); //get the vertex location in world space coordinates.

                float angle1 = Mathf.Atan2(((other.y-me.y)-offset),((other.x-me.x)-offset));// calculate the angle of the two offsets, to be stored in the structs.
                float angle3 = Mathf.Atan2(((other.y-me.y)+offset),((other.x-me.x)+offset));
                
                RaycastHit hit; //create and fire the two rays from the center of the light source in the direction of the vertex, with offsets.
                Physics.Raycast(transform.position, new Vector2( (other.x-me.x)-offset , (other.y-me.y)-offset ) , out hit, 100);
                RaycastHit hit2;
                Physics.Raycast(transform.position, new Vector2( (other.x-me.x)+offset , (other.y-me.y)+offset ), out hit2, 100);

                //store the hit locations as vertices in the struct, in model coordinates, as well as the angle of the ray cast and the UV at the vertex.
                angleds[(h*2)].vert = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                angleds[(h*2)].angle = angle1;
                angleds[(h*2)].uv = new Vector2(angleds[(h*2)].vert.x, angleds[(h*2)].vert.y);

			    angleds[(h*2)+1].vert = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(hit2.point);
                angleds[(h*2)+1].angle = angle3;
                angleds[(h*2)+1].uv = new Vector2(angleds[(h*2)+1].vert.x, angleds[(h*2)+1].vert.y);

                h++;//increment h.

                if(showRed && hit.collider != null)//for debugging: draw the rays cast.
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);		
                }
                if(showGreen)
                {
                    Debug.DrawLine(transform.position, hit2.point, Color.green);		
                }	

		    }
        }
        
        Array.Sort(angleds, delegate(angledVerts one, angledVerts two) {
                    return one.angle.CompareTo(two.angle);
                  });//sort the struct array of vertices from smallest angle to greatest.

        for (int i = 0; i < angleds.Length; i++)//store the values in the struct array in verts and uvs. 
        {                                       //(offsetting one because index 0 is the center of the light source and triangle fan)
            verts[i+1] = angleds[i].vert;
            uvs[i+1] = angleds[i].uv;
        }

		mesh.vertices = verts; //update the actual mesh with the new vertices.

        for (int i = 0; i < uvs.Length; i++)//offset all the UVs by .5 on both s and t to make the texture center be at the object center.
        {
            uvs[i] = new Vector2 (uvs[i].x + .5f, uvs[i].y + .5f);
        }

        mesh.uv = uvs; //update the actual mesh with the new UVs.
        
		int[] triangles = {0,1,verts.Length-1}; //init the triangles array, starting with the last triangle to orient normals properly.

		for (int i = verts.Length-1; i > 0; i--) //add all triangles to the triangle array, determined by three verts in the vertex array.
		{
			triangles = AddItemsToArray(triangles, 0, i, i-1);
		}
        //triangles = AddItemsToArray(triangles, 0, 1, 2);

		mesh.triangles = triangles; //update the actual mesh with the new triangles.
  	}
}