// Kinect-GRT.cpp : Defines the entry point for the console application.
// Targeting:	 x64 only
// Description:	 Connects to the kinect sensor and get the coordinates of the tracked joints in 3D space.

#include <iostream>
#include <cmath>
#include <cstdio>
#include <thread>
#include <atlstr.h>
#include <string>

#include <Windows.h>

//required for kinect 
#include <Ole2.h>
#include <Kinect.h>
#include <Kinect.Face.h>
#include "stdafx.h"

IKinectSensor* sensor;				// Kinect sensor
IMultiSourceFrameReader* reader;	// Kinect data source

//Frame sources
IBodyFrameSource* pBodyFrameSource = nullptr;				 
IFaceFrameSource* pFaceFrameSource = nullptr;	 

//Frame readers
IBodyFrameReader* pBodyFrameReader;
IFaceFrameReader* pFaceFrameReader;

//Mappers
ICoordinateMapper* mapper;			// Converts between depth, color, and 3d coordinates



//Define the face frame features to be tracked
static const DWORD c_FaceFrameFeatures = FaceFrameFeatures::FaceFrameFeatures_BoundingBoxInColorSpace
| FaceFrameFeatures::FaceFrameFeatures_PointsInColorSpace
| FaceFrameFeatures::FaceFrameFeatures_RotationOrientation
| FaceFrameFeatures::FaceFrameFeatures_Happy
| FaceFrameFeatures::FaceFrameFeatures_RightEyeClosed
| FaceFrameFeatures::FaceFrameFeatures_LeftEyeClosed
| FaceFrameFeatures::FaceFrameFeatures_MouthOpen
| FaceFrameFeatures::FaceFrameFeatures_MouthMoved
| FaceFrameFeatures::FaceFrameFeatures_LookingAway
| FaceFrameFeatures::FaceFrameFeatures_Glasses
| FaceFrameFeatures::FaceFrameFeatures_FaceEngagement;

// Body tracking results
BOOLEAN bodyTracked;					// Whether we see a body
Joint joints[JointType_Count];			// List of joints in the tracked body

//Face tracking result
//BOOLEAN faceTracked;					// Whether we see a face
DetectionResult* pfaceProperties[FaceProperty::FaceProperty_Count];

//function prototypes
bool initKinect();
void getBodyData(IMultiSourceFrame* frame);
void getFaceData1();
std::string formatFaceData(DetectionResult *FaceProperties);

int main()
{

	if (initKinect()) {
		std::cout << "Connected to kinect service";
		while (true) {

			//process body frames
			IMultiSourceFrame* frame = NULL;
			//if (SUCCEEDED(reader->AcquireLatestFrame(&frame))) {
				//getBodyData(frame);

				//check if body is being tracked
				//if (bodyTracked) {
					//body is tracked so write out joint positions
					//std::cout << joints[JointType_WristRight].Position.X;
					//std::cout << ",";
					//std::cout << joints[JointType_WristRight].Position.Y;
					//std::cout << ",";
					//std::cout << joints[JointType_WristRight].Position.Z;
					//std::cout << "\n";
			//	}
			//}

			//process face frames
			std::cout << "here1";
			//if (SUCCEEDED(pFaceFrameReader->AcquireLatestFrame(&faceframe))) {
				std::cout << "here 2";
				getFaceData1();
			}

				std::this_thread::sleep_for(std::chrono::milliseconds(100));
				//system("cls");

			}
		//}
}



//initilises the kinect sensor
bool initKinect() {

	//try to connect to sensor 
	if (FAILED(GetDefaultKinectSensor(&sensor))) {
		return false;
	}

	if (sensor) {

		//create base frame sources from sensor 
		IColorFrameSource* pColorFrameSource = nullptr;
		IBodyFrameSource* pBodyFrameSource = nullptr;
		
		sensor->Open();

		//connect sources to sensor 
		sensor->get_CoordinateMapper(&mapper);
		sensor->get_ColorFrameSource(&pColorFrameSource);

		sensor->OpenMultiSourceFrameReader(
			FrameSourceTypes::FrameSourceTypes_Depth | FrameSourceTypes::FrameSourceTypes_Color | FrameSourceTypes::FrameSourceTypes_Body,
			&reader);

		sensor->get_BodyFrameSource(&pBodyFrameSource);
		pBodyFrameSource->OpenReader(&pBodyFrameReader);

		//create face frame readers from sensor
		CreateFaceFrameSource(sensor, 0, c_FaceFrameFeatures, &pFaceFrameSource);
		pFaceFrameSource->OpenReader(&pFaceFrameReader);

		//no longer require these as they are embedded in readers
		SafeRelease(pColorFrameSource);
		SafeRelease(pBodyFrameSource);
		return true;
	}
	else {
		return false;
	}
}

//gets the postion of all the joints as 3D coordinates 
void getBodyData(IMultiSourceFrame* frame) {
	IBodyFrame* bodyframe;
	IBodyFrameReference* frameref = NULL;
	frame->get_BodyFrameReference(&frameref);
	frameref->AcquireFrame(&bodyframe);
	if (frameref) frameref->Release();

	if (!bodyframe) return;

	IBody* body[BODY_COUNT] = { 0 };
	bodyframe->GetAndRefreshBodyData(BODY_COUNT, body);
	for (int i = 0; i < BODY_COUNT; i++) {
		body[i]->get_IsTracked(&bodyTracked);
		if (bodyTracked) {
			body[i]->GetJoints(JointType_Count, joints);
			break;
		}
		
	}

	if (bodyframe) bodyframe->Release();
}

bool UpdateBodyData(IBody** ppBody) {

	if (pBodyFrameReader != nullptr) {
		IBodyFrame* pBodyFrame = nullptr;
		if (SUCCEEDED(pBodyFrameReader->AcquireLatestFrame(&pBodyFrame))) {
			pBodyFrame->GetAndRefreshBodyData(1, ppBody);
			return true;
		}
		else {
			return false;
		}
		SafeRelease(pBodyFrame);
	}
}


void getFaceData1() {

	IBody* ppBodies[BODY_COUNT] = { 0 };
	bool bHaveBodyData = SUCCEEDED(UpdateBodyData(ppBodies));
	BOOLEAN bFaceTracked = false;

	IFaceFrame* pFaceFrame = nullptr;
	//test for tracked face
	if (SUCCEEDED(pFaceFrameReader->AcquireLatestFrame(&pFaceFrame))) {
		pFaceFrame->get_IsTrackingIdValid(&bFaceTracked);
	}
	
	if (bHaveBodyData) {
		IBody* pBody = ppBodies[0];
		
		if (pBody != nullptr) {
			BOOLEAN bTracked = false;
			pBody->get_IsTracked(&bTracked);

			UINT64 bodyTId;
			if (bTracked) {
				if SUCCEEDED(pFaceFrameSource->put_TrackingId(bodyTId)) {
					pFaceFrameSource->put_TrackingId(bodyTId);
				}
			}
		}
	}
	
	//face is tracked so process it
	if (bFaceTracked) {
		IFaceFrameResult* pFaceFrameResult = nullptr;
		pFaceFrame->get_FaceFrameResult(&pFaceFrameResult);
		pFaceFrameResult->GetFaceProperties(FaceProperty::FaceProperty_Count, *pfaceProperties);
		formatFaceData(*pfaceProperties);
	}
	SafeRelease(pFaceFrame);
}


std::string formatFaceData(DetectionResult* FaceProperties) {
	
	std::cout << "here3";
	std::wstring faceText = L"";
	std::string result;

	//extract each face property and store it in faceText
	for (int iProperty = 0; iProperty < FaceProperty::FaceProperty_Count; iProperty++)
	{
		switch (iProperty)
		{
		case FaceProperty::FaceProperty_Happy:
			faceText += L"Happy :";
			break;
		case FaceProperty::FaceProperty_Engaged:
			faceText += L"Engaged :";
			break;
		case FaceProperty::FaceProperty_LeftEyeClosed:
			faceText += L"LeftEyeClosed :";
			break;
		case FaceProperty::FaceProperty_RightEyeClosed:
			faceText += L"RightEyeClosed :";
			break;
		case FaceProperty::FaceProperty_LookingAway:
			faceText += L"LookingAway :";
			break;
		case FaceProperty::FaceProperty_MouthMoved:
			faceText += L"MouthMoved :";
			break;
		case FaceProperty::FaceProperty_MouthOpen:
			faceText += L"MouthOpen :";
			break;
		case FaceProperty::FaceProperty_WearingGlasses:
			faceText += L"WearingGlasses :";
			break;
		default:
			break;
		}

		switch (*pfaceProperties[iProperty])
		{
		case DetectionResult::DetectionResult_Unknown:
			faceText += L" UnKnown";
			break;
		case DetectionResult::DetectionResult_Yes:
			faceText += L" Yes";
			break;

		case DetectionResult::DetectionResult_No:
		case DetectionResult::DetectionResult_Maybe:
			faceText += L" No";
			break;
		default:
			break;
		}

		faceText += L"\n";

		for (char x : faceText) {
			result += x;
		}
		std::cout << result;
	}
	return result;
}