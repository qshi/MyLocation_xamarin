# My Location - Indoor/Outdoor location information gathering system


## User Application
The My Shoppe consumer application enables you as a shop owner to easily connect with your consumer base. This application  enables you to create a browesable list of landmarks locations so users can find the closest location to them, call the officer, see hours, and even navigate to the landmark with a single click.

### Features
* Browse Landmarks / Locations (with online/offline sync)
* Location Information: address, phone, hours, and more
* Navigate to Location
* Call Location with 1 click
* Leave feedback for missing landmarks


## Admin Application
In addition to the consumer application that you can release into the app stores, I created an Administration mobile application that allows you to manage all your shops and feedback from customers. It uses the same backend from the consumer application and even shares some of the same UI and code. 

### Features
* Create and Manage your Shops
* Browse & Manage Feedback from consumers
* Call back consumers with a single click


##1.	Objective
The goal is to design an iPhone application to help people get to know landmarks position  information indoors/outdoors accurately combining GPS and Bluetooth localization. The application development includes building a real-time data updating system based on Azure including online report and offline information verification & update.  
##2.	Assumptions
Users use the My Location App to localize themselves, get landmarks alert and Report the missing landmarks location.
Administrator use Location Admin App to review all the reports, verify information onsite and install iBeacons around the reported zone.

##3.	Landmarks: User report
a)	Work zone
b)	Traffic signal
c)	Round about
d)	Crosswalk
e)	Bus station
f)	Buildings entrance 


##4.	System Architecture
 
##5.	Implementation
(1)	User Application
There are three functions in the user application: My Location, Report, Landmarks Position (verified).
My Location: Provide location and navigation service for users
 		 

Report: Users can use this to submit missing landmarks report
 

Select location on the map through long tap
 		 
Landmarks Position: verified landmarks location list
Click each items can view detailed description. For example: location, bus hours etc.
 
(2)	Admin Application 
Report Map: Admin can view all the landmarks location from users reports 
 	 

View report: Admin can view the reports submitted from all the users. When click each items, admin can view detailed description
 	 

Manage Location: After verifying the landmarks location admin can install ibeacons and add/modify landmarks
 		 
	

##6.	Future work
Positioning and notification method

1. BLE deployment: Region Boundary definition
Add polylines between ibeacons with same major id(work zone) until there is a polygon which is the region of work zone.
For example:  

2. Switch between BLE and GPS localization method
When GPS accuracy is less than specific value and iphone can receive more than three ibeacons signal (rssi larger than specific value), the app will combine GPS localization with BLE localization method using factor α.

The positioning information by BLE is used to correct the error of GPS if the system can read BLE tags. 
1.	Let Vector PBLE denotes the position vector of BLE and let Vector PGPS denotes the position vector of GPS at the same time. 
2.	Then the correction vector C is obtained by vector PBLE-PGPS  which is a offset between vector PBLE and vector PGPS. 
3.	When the position information by GPS is updated to vector P’GPS, the estimated position is calculated as vector P=vector P’GPS+ vector C. 
Based on above, we introduce the factor α to the correction vector C. 
Define :  vector P= vector P’GPS + αC

The system gradually reduce α according to the elapsed time from the last read time of the measurement data from 1→0

factor α = (received ibeacons number-3)* beacons signal value/ (received ibeacons number-3)*STRONG VALUE;











