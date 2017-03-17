using csmatio.io;
using csmatio.types;
using KinectServer.Common;
using KinectServer.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KinectServer.BusinessLogic
{
    public static class MatlabDriver
    {
        #region Feilds

        private const int _jointsTimesCoordinate = 25*3;
        private const int _jointsTimesCoordinate2d = 25 * 2;
        private static string _folder;

        #endregion 
        
        #region Public Methods

        public static void SetFolder()
        {
            var matlabFolder = ConfigurationManager.AppSettings["matlabFolder"];
            var path = matlabFolder + DateTime.Now.ToString("dd-MM-yyyy h_mm_ss");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _folder = path;
        }
        public static event Action<string> WritingActionStateChanged;


        public static void ToMat(List<List<Point3D>> skeleton, string name = null)
        {
            Task.Factory.StartNew(() =>
            {
                MLStructure sequenceStruct = new MLStructure("Skeletons", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                MLSingle jointsMat = new MLSingle("", new int[] { _jointsTimesCoordinate, skeleton.Count });
                int frameCount = 0;
                foreach (var frame in skeleton)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat.Set(point.X, jointCount * 3, frameCount);
                        jointsMat.Set(point.Y, jointCount * 3 + 1, frameCount);
                        jointsMat.Set(point.Z, jointCount * 3 + 2, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }
                sequenceStruct["Skeletons"] = jointsMat;
                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "skeletonData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }

                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });
        }

        public static void ToMatRandom(List<List<Point3D>> skeleton, List<Hit> hits, string name = null)
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("RandomGame", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                MLSingle jointsMat = new MLSingle("", new int[] { _jointsTimesCoordinate, skeleton.Count });
                int frameCount = 0;
                foreach (var frame in skeleton)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat.Set(point.X, jointCount * 3, frameCount);
                        jointsMat.Set(point.Y, jointCount * 3 + 1, frameCount);
                        jointsMat.Set(point.Z, jointCount * 3 + 2, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }
                sequenceStruct["Skeletons"] = jointsMat;
                if (hits.Count > 0)
                {
                    MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                    int hitsCounter = 0;
                    foreach (var hit in hits)
                    {
                        hitsMat.Set(hit.T1, 0, hitsCounter);
                        hitsMat.Set(hit.T2, 1, hitsCounter);
                        hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                    }
                    sequenceStruct["hits"] = hitsMat;
                }
                

                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "skeletonData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }

                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });
        }

        public static void ToMat(List<Hit> hits, string name = null)
        {
            Task.Factory.StartNew(() =>
            {
               
                MLStructure sequenceStruct = new MLStructure("hits", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                int hitsCounter = 0;
                foreach (var hit in hits)
                {
                    hitsMat.Set(hit.T1,0, hitsCounter);
                    hitsMat.Set(hit.T2,1, hitsCounter);
                    hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                }

                sequenceStruct["hits"] = hitsMat;
                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "hitsData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }


                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });


        }

        public static void ToMatAll(List<List<Point3D>> skeleton, List<Hit> hits, List<Point3D> trajectories, List<List<ScreenPoint>> skeleton2d, List<ScreenPoint> trajectories2d,List<int> statesCues, string name = "")
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("Game", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", name);
                
                MLStructure metadataStruct = new MLStructure("Metadata", new int[] { 1, 1 });
                metadataStruct["CuesDbFilename"] = new MLChar("",ConfigurationManager.AppSettings["enemiesFile"]);
                sequenceStruct["Metadata"] = metadataStruct;
                
                MLSingle cuesMat = new MLSingle("", new int[] { 1, statesCues.Count });

                for (int i = 0; i < statesCues.Count; i++)
                {
                    cuesMat.Set(statesCues[i], i);
                }

                sequenceStruct["Cues"] = cuesMat;

                MLSingle jointsMat = new MLSingle("", new int[] { _jointsTimesCoordinate, skeleton.Count });
                MLSingle jointsMat2d = new MLSingle("", new int[] { _jointsTimesCoordinate2d, skeleton2d.Count });
                int frameCount = 0;
                foreach (var frame in skeleton)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat.Set(point.X, jointCount * 3, frameCount);
                        jointsMat.Set(point.Y, jointCount * 3 + 1, frameCount);
                        jointsMat.Set(point.Z, jointCount * 3 + 2, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }
                frameCount = 0;
                foreach (var frame in skeleton2d)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat2d.Set(point.X, jointCount * 2, frameCount);
                        jointsMat2d.Set(point.Y, jointCount * 2 + 1, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }

                sequenceStruct["Skeletons"] = jointsMat;
                sequenceStruct["Skeletons2d"] = jointsMat2d;

                if (hits.Count > 0)
                {
                    int maxTraj = 0;

                    MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                    int hitsCounter = 0;
                    foreach (var hit in hits)
                    {
                        hitsMat.Set(hit.T1, 0, hitsCounter);
                        hitsMat.Set(hit.T2, 1, hitsCounter);
                        hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                    }

                    sequenceStruct["hits"] = hitsMat;
                }

                if (trajectories.Count > 0)
                {
                    MLSingle trajMat = new MLSingle("", new int[] { 3, trajectories.Count });
                    MLSingle trajMat2d = new MLSingle("", new int[] { 2, trajectories2d.Count });
                    int trajCounter = 0;
                    foreach (var t in trajectories)
                    {
                        trajMat.Set(t.X, 0, trajCounter);
                        trajMat.Set(t.Y, 1, trajCounter);
                        trajMat.Set(t.Z, 2, trajCounter++);
                    }

                    trajCounter = 0;
                    foreach (var t in trajectories2d)
                    {
                        trajMat2d.Set(t.X, 0, trajCounter);
                        trajMat2d.Set(t.Y, 1, trajCounter++);
                        
                    }

                    sequenceStruct["trajectories"] = trajMat;
                    sequenceStruct["trajectories2d"] = trajMat2d;
                }

                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "skeletonData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }

                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });
        }

        public static void ToMatMoving(List<Hit> hits,TrajectoriesData trajectories, string name = null)
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("hits", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                int maxTraj = 0;

                MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                int hitsCounter = 0;
                foreach (var hit in hits)
                {
                    hitsMat.Set(hit.T1, 0, hitsCounter);
                    hitsMat.Set(hit.T2, 1, hitsCounter);
                    hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                }

                sequenceStruct["hits"] = hitsMat;

                MLStructure trajectoryStruct = new MLStructure("hits", new int[] { 1, 1 });
                trajectoryStruct["UserName", 0] = new MLChar("", "igork");

                
                MLSingle trajMat = new MLSingle("", new int[] { 3, trajectories.Moving.Count });
                int trajCounter = 0;
                foreach (var t in trajectories.Moving)
                {
                    trajMat.Set(t.X, 0, trajCounter);
                    trajMat.Set(t.Y, 1, trajCounter);
                    trajMat.Set(t.Z, 2, trajCounter++);
                }

                sequenceStruct["trajectories"] = trajMat;



                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);
                
                string filename = "hitsData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }


                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });


        }

        #endregion 
    }
}
