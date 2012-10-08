using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_v1._1
{



    class DTWRecognition
    {
        private Gestures sessionGestures;
        private Gestures libraryGestures;

        private Dictionary<GestureKey, List<List<float>>> session;
        private Dictionary<GestureKey, List<List<float>>> library;

        private const int vectCount = 3;
        private float globalThreshold;
        private int maxSlope;

        public DTWRecognition(Gestures sessionGestures, Gestures libraryGestures, float globalThreshold, int maxSlope)
        {
            this.sessionGestures = sessionGestures;
            this.libraryGestures = libraryGestures;
            this.globalThreshold = globalThreshold;
            this.maxSlope = maxSlope;

            library = new Dictionary<GestureKey, List<List<float>>>(libraryGestures.gestures.Count);
            session = new Dictionary<GestureKey, List<List<float>>>(sessionGestures.gestures.Count);

            transformData();

        }

//========================================================================= Classification Functions ==========================================================================================================

        public Dictionary<GestureKey, Dictionary<GestureKey, List<float>>> classify()
        {
            Dictionary<GestureKey, Dictionary<GestureKey, List<float>>> session_dtwResults = new Dictionary<GestureKey, Dictionary<GestureKey, List<float>>>(session.Count);

            foreach (KeyValuePair<GestureKey, List<List<float>>> session_kvp in session)
            {
                Dictionary<GestureKey, List<float>> dtwResults = new Dictionary<GestureKey, List<float>>(library.Count);
                foreach (KeyValuePair<GestureKey, List<List<float>>> library_kvp in library)
                {
                    List<float> values = new List<float>();
                    for (int jointIndex = 2; jointIndex < 62; jointIndex += vectCount)
                    {
                        var sessionJoint = session_kvp.Value.GetRange(jointIndex, vectCount);
                        var libraryJoint = library_kvp.Value.GetRange(jointIndex, vectCount);
                        float d = dtw(sessionJoint, libraryJoint);
                        values.Add(d);
                    }
                    float average = values.Average();
                    values.Add(average);
                    dtwResults.Add(library_kvp.Key, values);
                }
                session_dtwResults.Add(session_kvp.Key, dtwResults);
            }
            return session_dtwResults;
        }

        private float dtw(List<List<float>> session_seq,List<List<float>> library_seq)
        {
            // Init
            var session_seqR = new List<List<float>>(session_seq);
            session_seqR.Reverse();
            var library_seqR = new List<List<float>>(library_seq);
            library_seqR.Reverse();
            var tab = new float[session_seqR.Count + 1, library_seqR.Count + 1];
            var slopeI = new int[session_seqR.Count + 1, library_seqR.Count + 1];
            var slopeJ = new int[session_seqR.Count + 1, library_seqR.Count + 1];

            for (int i = 0; i < session_seqR.Count + 1; i++)
            {
                for (int j = 0; j < library_seqR.Count + 1; j++)
                {
                    tab[i, j] = float.PositiveInfinity;
                    slopeI[i, j] = 0;
                    slopeJ[i, j] = 0;
                }
            }

            tab[0, 0] = 0;

            // Dynamic computation of the DTW matrix.
            for (int i = 1; i < session_seqR.Count + 1; i++)
            {
                for (int j = 1; j < library_seqR.Count + 1; j++)
                {
                    if (tab[i, j - 1] < tab[i - 1, j - 1] && tab[i, j - 1] < tab[i - 1, j] &&
                        slopeI[i, j - 1] < maxSlope)//deletion
                    {
                        tab[i, j] = Dist2(session_seqR[i - 1],library_seqR[j - 1]) + tab[i, j - 1];
                        slopeI[i, j] = slopeJ[i, j - 1] + 1;
                        slopeJ[i, j] = 0;
                    }
                    else if (tab[i - 1, j] < tab[i - 1, j - 1] && tab[i - 1, j] < tab[i, j - 1] &&
                             slopeJ[i - 1, j] < maxSlope)//insertion
                    {
                        tab[i, j] = Dist2(session_seqR[i - 1],library_seqR[j - 1]) + tab[i - 1, j];
                        slopeI[i, j] = 0;
                        slopeJ[i, j] = slopeJ[i - 1, j] + 1;
                    }
                    else
                    {//match
                        tab[i, j] = Dist2(session_seqR[i - 1],library_seqR[j - 1]) + tab[i - 1, j - 1];
                        slopeI[i, j] = 0;
                        slopeJ[i, j] = 0;
                    }
                }
            }

            // Find best between library_seq and an ending (postfix) of session_seq.
            float bestMatch = float.PositiveInfinity;
            for (int i = 1; i < (session_seqR.Count + 1); i++)
            {
                if (tab[i, library_seqR.Count] < bestMatch)
                {
                    bestMatch = tab[i, library_seqR.Count];
                }
            }

            return bestMatch;
        }

        private float Dist1(List<float> a, List<float> b)
        {
            float d = 0;
            for (int i = 0; i < vectCount; i++)
            {
                d += Math.Abs(a[i] - b[i]);
            }

            return d;
        }

        private float Dist2(List<float> a, List<float> b)
        {
            float d = 0;
            for (int i = 0; i < vectCount; i++)
            {
                d += (float) Math.Pow(a[i] - b[i], 2);
            }

            return (float) Math.Sqrt(d);
        }

        



















//========================================================================= Transform Data Functions ==========================================================================================================


        private void transformData()
        {
            foreach (KeyValuePair<GestureKey, List<float[]>> kvp in libraryGestures.gestures)
            {
                GestureKey key = kvp.Key;
                List<List<float>> value = toListList(standardise(kvp.Value));
                library.Add(key, value);
            }

            foreach (KeyValuePair<GestureKey, List<float[]>> kvp in sessionGestures.gestures)
            {
                GestureKey key = kvp.Key;
                List<List<float>> value = toListList(standardise(kvp.Value));
                session.Add(key, value);               
            }
        }


        public List<List<float>> toListList(List<float[]> listArray)
        {
            List<List<float>> listList = new List<List<float>>(62);

            for (int j = 0; j < 62; j++)
            {
                List<float> list = new List<float>(listArray.Count);

                for (int i = 0; i < listArray.Count; i++)
                {
                    list.Add(listArray[i][j]);
                }

                listList.Add(list);
            }
            return listList;
        }

        private float[] average(List<float[]> dataList)
        {
            float[] avgs = new float[62];

            for (int j = 2; j < 62; j++)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    avgs[j] += dataList[i][j];
                }
                avgs[j] /= dataList.Count;
            }

            return avgs;
        }

        private float[] variance(List<float[]> dataList)
        {
            float[] vars = new float[62];
            float[] avgs = average(dataList);

            for (int j = 2; j < 62; j++)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    vars[j] += (dataList[i][j] - avgs[j]) * (dataList[i][j] - avgs[j]);
                }
                vars[j] /= dataList.Count;
            }

            return vars;
        }

        private List<float[]> standardise(List<float[]> dataList)
        {
            float[] avgs = average(dataList);
            float[] vars = variance(dataList);
            

            for (int j = 2; j < 62; j++)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i][j] = (dataList[i][j] - avgs[j])/(float)(Math.Sqrt(vars[j]));
                }
            }

            return dataList;
        }

    }
}
