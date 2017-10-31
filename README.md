# FaceRecognition-webcam
Using the Microsoft Face API to demo face detection and face recognition - a piece of work in progress

A basic C# asp.net MVC project to demonstrate the face detection and face recognition features of the Microsoft Face API, with a webcam.

## Face Detection
The abiltiy to detect a face or multiple faces in a image.
The user can look into webcam and click button. Image is captured and passed to the Face API. The first face detected will have a square drawn on the image to indicate face location. Face API currently allows detection of upto 64 faces in an image so will add ability to detect multiple faces soon.
A range of face attributes are detected along with passing of the "face rectangle" to the Emotion API to return emotion attributes.

## Face Recognition
The ability to determine that a face in an image belongs to a particular person.
The user can look into webcam and click button (also added in annyang support to allow user to speak to browser to activate). Image is captured and passed to the Face API. The Face API will compare the face to those set up in the People Group section and return the Person if a match is found.


