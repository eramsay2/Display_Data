using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows;

public class MocapLoader : MonoBehaviour
{
    int iainsFrameNo = 0;
    int iainsFrameCount = 0;

    public Transform[] joints; // Assign your cubes (joints) in the Inspector
    private List<Vector3[]> motionData = new List<Vector3[]>(); // Stores all the frames of motion data
    List<sJointData> lJoints = new List<sJointData>();
    List<sJointData>[] lJointsWithID = new List<sJointData>[52];


    string fullFileAndPath = "";

    public struct sJointData
    {
        public long timestamp;       // Timestamp
        public int jointID;     // Joint name (e.g., "knee", "elbow")
        public int frame;        // Frame number
        public Vector3 position; // Position data

        public sJointData(long timestamp, int jointID, int frame, Vector3 position)
        {
            this.timestamp = timestamp;
            this.jointID = jointID;
            this.frame = frame;
            this.position = position;
        }

        // Optional: override ToString() for easy debugging
        public override string ToString()
        {
            return $"[Frame {frame} | Time {timestamp} ticks | Joint: {jointID} | Pos: {position}]";
        }
    }

    int maxFrame = 0; 

    void Start()
    {

        string fileName = "test.0001.Joints"; // Your CSV file name (without .csv extension)
        Debug.Log(Application.persistentDataPath);

        fullFileAndPath = Application.persistentDataPath + @"\" + fileName; //+".csv"
        Debug.Log(fullFileAndPath);

        lJoints=  LoadMocapData(fileName);

        maxFrame = GetMaxFrame(lJoints);

        lJointsWithID = GetListOfJointsWithID(lJoints, maxFrame);


        StartCoroutine(AnimateSkeleton(lJointsWithID,maxFrame));
    }

    List<sJointData>[] GetListOfJointsWithID(List<sJointData> lINPUT, int MAXFRAME)
    {
        List<sJointData>[] lOUT = new List<sJointData>[52];
        for (int j = 0; j < 52; j++) lOUT[j] = new List<sJointData>(); //Necessary when you have an array of lists

        for (int i = 0; i < lINPUT.Count; i++)
        {
            sJointData sCOL = new sJointData();
            sCOL = lINPUT[i];
            lOUT[sCOL.jointID].Add(sCOL);
        }

    return lOUT;
    }


    List<sJointData> LoadMocapData(string fileName)
    {
        List<sJointData> lOUT = new List<sJointData>();

        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources folder!");
            return lOUT;
        }

        string[] lines = csvFile.text.Split('\n');
        Debug.Log(lines[0]);

        List<Vector3> currentFrame = new List<Vector3>();

        foreach (string line in lines)
        {
            string[] values = line.Split(',');
            Debug.Log("VALUES LENGHT " + values.Length);

            // Skip empty lines or lines with insufficient data
            if (values.Length == 6)
            {

                long timestamp = long.Parse(values[0]); // Timestamp (ignored)
              //  Debug.Log("timestamp " + " " + timestamp);
                int frame = int.Parse(values[1]); // Frame number (ignored)
                int jointID = int.Parse(values[2]); // Joint ID

                // Extract the X, Y, Z values for this joint
                float x = float.Parse(values[3]);
                float y = float.Parse(values[4]);
                float z = float.Parse(values[5]);


                sJointData sCOL  = new sJointData();
                sCOL.frame = frame;
                sCOL.timestamp = timestamp;
                sCOL.jointID = jointID;
                sCOL.position.x = x; 
                sCOL.position.y = y;
                sCOL.position.z = z;
                lOUT.Add(sCOL);


                // Ensure we have a valid joint ID
                //if (jointID > 0 && jointID <= joints.Length)
                //{
                //    Vector3 jointPosition = new Vector3(x, y, z);

                //    // If we're in the same frame, add this joint position to the current frame
                //    if (frame > currentFrame.Count)
                //    {
                //        currentFrame.Add(jointPosition);
                //    }
                //    //else
                //    //{
                //    //    currentFrame[jointID - 1] = jointPosition; // Update position of this joint in the current frame
                //    //}
                //}

                // Once all joints are assigned for this frame, store the frame data
                if (currentFrame.Count == joints.Length)
                {
                    motionData.Add(currentFrame.ToArray());
                    currentFrame.Clear();
                }

                iainsFrameCount++;
            }
        }
        Debug.Log("NUMBER OF LINES: " + iainsFrameCount);

        for (int i = 0;i< lOUT.Count; i++)
        {
         //   Debug.Log(lOUT[i].ToString());

        }


        return lOUT; 
    }

    private void Update()
    {
        iainsFrameNo++;
     //   Debug.Log(iainsFrameNo);
    }

    int GetMaxFrame(List<sJointData> lINPUT)
    {
        int maxFrame = 0;
        for (int i = 0; i < lINPUT.Count; i++)
        {
           if (lINPUT[i].frame > maxFrame)
            {
                maxFrame = lINPUT[i].frame;
            }
        }

        return maxFrame;
    }


    IEnumerator AnimateSkeleton(List<sJointData>[] lINPUT, int MAXFRAME)
    {
        while (true) // Loop the animation
        {
            for (int frame = 0; frame < MAXFRAME; frame++)
            {

                for (int j = 0; j < joints.Length; j++)
                {
                    Vector3 vTemp = lINPUT[j][frame].position;
                    joints[j].localPosition = 100*vTemp;
                }
                yield return new WaitForSeconds(1f / 30f); // Assuming 30 FPS
            }
        }
    }
}
