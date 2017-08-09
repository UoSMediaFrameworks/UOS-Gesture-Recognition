// Kinect-GRT.cpp : Defines the entry point for the console application.
// Targeting:	 x64 only
// Description:	 Connects to the kinect sensor and get the coordinates of the tracked joints in 3D space.

#include <iostream>
#include <cmath>
#include <cstdio>


#include <Windows.h>
#include <Ole2.h>


#include <Kinect.h>
#include <thread>


IKinectSensor* sensor;				// Kinect sensor
IMultiSourceFrameReader* reader;	// Kinect data source
ICoordinateMapper* mapper;			// Converts between depth, color, and 3d coordinates

// Body tracking variables
BOOLEAN tracked;					// Whether we see a body
Joint joints[JointType_Count];		// List of joints in the tracked body

//function prototypes
bool initKinect();
void getBodyData(IMultiSourceFrame* frame);

int main()
{
	if (initKinect()) {
		std::cout << "Connected to kinect service";
		while (true) {
			std::this_thread::sleep_for(std::chrono::milliseconds(100));
			
			IMultiSourceFrame* frame = NULL;
			if (SUCCEEDED(reader->AcquireLatestFrame(&frame))) {
				getBodyData(frame);

				//check if body is being tracked
				if (tracked) {
					//body is tracked so write out joint positions
					std::cout << joints[JointType_WristRight].Position.X;
					std::cout << ",";
					std::cout << joints[JointType_WristRight].Position.Y;
					std::cout << ",";
					std::cout << joints[JointType_WristRight].Position.Z;
					std::cout << "\n";
				}
			}
		}
	}
}


//initilises the kinect sensor
bool initKinect() {
	if (FAILED(GetDefaultKinectSensor(&sensor))) {
		return false;
	}
	if (sensor) {
		sensor->get_CoordinateMapper(&mapper);

		sensor->Open();
		sensor->OpenMultiSourceFrameReader(
			FrameSourceTypes::FrameSourceTypes_Depth | FrameSourceTypes::FrameSourceTypes_Color | FrameSourceTypes::FrameSourceTypes_Body,
			&reader);
		return reader;
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
		body[i]->get_IsTracked(&tracked);
		if (tracked) {
			body[i]->GetJoints(JointType_Count, joints);
			break;
		}
	}

	if (bodyframe) bodyframe->Release();
}