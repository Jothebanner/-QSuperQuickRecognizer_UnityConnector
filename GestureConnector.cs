// Jacob Hawks
// 11/24/2021
// Some of this code is directly used from the $Q Point-Cloud Recognizer software, other bits are modified versions of the code.

/*
 * Mad respect for Radu-Daniel Vatavu, Lisa Anthony, and Jacob O. Wobbrock
 * 
 * A poem in their honor:
 * 
 * Roses are red
 * Violets are blue
 * Your software is gorgeous
 * Just like you :)
 * 
 * Amen
 */

// Disclaimer from the source:

/**
 * The $Q Point-Cloud Recognizer (.NET Framework C# version)
 *
 * 	    Radu-Daniel Vatavu, Ph.D.
 *	    University Stefan cel Mare of Suceava
 *	    Suceava 720229, Romania
 *	    radu.vatavu@usm.ro
 *
 *	    Lisa Anthony, Ph.D.
 *      Department of CISE
 *      University of Florida
 *      Gainesville, FL 32611, USA
 *      lanthony@cise.ufl.edu
 *
 *	    Jacob O. Wobbrock, Ph.D.
 * 	    The Information School
 *	    University of Washington
 *	    Seattle, WA 98195-2840
 *	    wobbrock@uw.edu
 *
 * The academic publication for the $Q recognizer, and what should be 
 * used to cite it, is:
 *
 *	Vatavu, R.-D., Anthony, L. and Wobbrock, J.O. (2018).  
 *	  $Q: A Super-Quick, Articulation-Invariant Stroke-Gesture
 *    Recognizer for Low-Resource Devices. Proceedings of 20th International Conference on
 *    Human-Computer Interaction with Mobile Devices and Services (MobileHCI '18). Barcelona, Spain
 *	  (September 3-6, 2018). New York: ACM Press.
 *	  DOI: https://doi.org/10.1145/3229434.3229465
 *
 * This software is distributed under the "New BSD License" agreement:
 *
 * Copyright (c) 2018, Radu-Daniel Vatavu, Lisa Anthony, and 
 * Jacob O. Wobbrock. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University Stefan cel Mare of Suceava, 
 *	    University of Washington, nor University of Florida, nor the names of its contributors 
 *	    may be used to endorse or promote products derived from this software 
 *	    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Radu-Daniel Vatavu OR Lisa Anthony
 * OR Jacob O. Wobbrock BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
**/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDollarGestureRecognizer;
using QDollarGestureRecognizer;
using System.Xml;
using System.IO;

public class GestureConnector : MonoBehaviour
{
	// TODO: make getters and setters
    [SerializeField] List<Gesture> templateGestures = new List<Gesture>();
    List<TextAsset> templateGesturesTextAssets = new List<TextAsset>();

    private void Awake()
    {
        TextAsset[] gestureTextAssets = LoadTemplateGestures();
        foreach (TextAsset gestureTextAsset in gestureTextAssets)
        {
            // I don't know why I'm storing these
            // TODO: work for smarter, future me lol
            templateGesturesTextAssets.Add(gestureTextAsset);

            // send each TextAsset gesture to be turned into a unity TextAsset gesture
            convertTextToGesture(gestureTextAsset);
        }
        Debug.Log(templateGestures.Count + " gestures loaded and added!");
    }

    public Point[] PointExtractor(List<Vector3[]> _strokeList)
	{
        List<Point> pointList = new List<Point>();

        int currentStroke = 0;
        // heaven knows why I didn't just use for loops.
        foreach (Vector3[] stroke in _strokeList)
        {
            int counter = 0;
            foreach (Vector3 position in stroke)
            {
                Point nextPoint = new Point(position.x, position.z, currentStroke);
                pointList.Add(nextPoint);
                counter++;
            }
            currentStroke++;
        }

        Debug.Log("Number of points: " + pointList.Count);

        return pointList.ToArray();
	}

    // TODO: Probably don't access directly idk
	public string CheckCandidate(Point[] _pointArray)
    {
		Gesture candidateGesture = new Gesture(_pointArray);
		return QPointCloudRecognizer.Classify(candidateGesture, templateGestures.ToArray());
	}

    // TODO: this should happen when the game is loading
	public TextAsset[] LoadTemplateGestures()
    {
		TextAsset[] tg = Resources.LoadAll<TextAsset>("GestureSet/");
        return tg;
    }

	void convertTextToGesture(TextAsset _gestureText)
	{
        // little test boi TODO: probably get rid of lol
        //if (templateGestures.Count == 1)
        //{
        //    pointArray = templateGestures[0].PointsRaw;
        //}





        // Gesture xml looks something like
        /*  
         *  <Gesture>
         *      <Stroke>
         *          <Point> *point info* </Point>
         *      or  <Point *point info* />
         *          *lots of points*....
         *          ...
         *          ...
         *          ...
         *      </Stroke>
         *  </Gesture>
         */

        List<Point> points = new List<Point>();
        int currentStrokeIndex = -1;
        string gestureName = "";
        try
        {
            XmlDocument gestureRawXML = new XmlDocument();
            gestureRawXML.LoadXml(_gestureText.text);

            // select the gesture node from the xml file
            XmlNode gestureNode = gestureRawXML.SelectSingleNode("/Gesture");

            // get the name of the gesture
            gestureName = gestureNode.Attributes["Name"].Value;

            // fix special characters
            if (gestureName.Contains("~")) // '~' character is specific to the naming convention of the MMG set
                gestureName = gestureName.Substring(0, gestureName.LastIndexOf('~'));
            if (gestureName.Contains("_")) // '_' character is specific to the naming convention of the MMG set
                gestureName = gestureName.Replace('_', ' ');

            // get all of the strokes nodes in this gesture
            XmlNodeList strokeList = gestureRawXML.SelectNodes("*/Stroke");

            foreach (XmlNode stroke in strokeList)
            {
                try
                {
                    // custom gestures do not have the stroke index property, but older ones do tho ->
                    // qrecognizer does not even use the stroke indices. I probably wanted to play with try catch idk lol
                    if (stroke.InnerText != "")
                        currentStrokeIndex = int.Parse(stroke.Attributes["index"].Value);
                }
                catch(System.Exception e)
                {
                    Debug.LogWarning(e);
                }

                // select all of the points in the stroke
                XmlNodeList pointList = stroke.SelectNodes("Point");

                foreach (XmlNode point in pointList)
                {
                    // add them to the points list (not pointList)
                    points.Add(new Point(float.Parse(point.Attributes["X"].Value), float.Parse(point.Attributes["Y"].Value), currentStrokeIndex));
                }
            }
        }
        catch (System.Exception e)
        { 
            Debug.LogWarning(e);
        }
        finally
        {
            Debug.Log(gestureName + " gesture Successfully added");
        }
        Gesture currentGesture = new Gesture(points.ToArray(), gestureName);
        
        // finnnnnnnally add the loaded gesture
        templateGestures.Add(currentGesture);
    }
}
