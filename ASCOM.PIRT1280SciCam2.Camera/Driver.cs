//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Camera driver for PIRT1280SciCam2
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Camera interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 2023-01-20	PPP	1.0.0	Basic working driver, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define Camera

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading;
using BufferAcquisition;
using CLComm;

namespace ASCOM.PIRT1280SciCam2
{
    //
    // Your driver's DeviceID is ASCOM.PIRT1280SciCam2.Camera
    //
    // The Guid attribute sets the CLSID for ASCOM.PIRT1280SciCam2.Camera
    // The ClassInterface/None attribute prevents an empty interface called
    // _PIRT1280SciCam2 from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Camera Driver for PIRT1280SciCam2.
    /// </summary>
    [Guid("991489dc-e04d-47b9-b24e-e0d788322e88")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Camera : ICameraV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.PIRT1280SciCam2.Camera";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "PIRT1280SciCam2";
        private static double exposureMinVal = 100e-6;

        internal static string comPortProfileName = "CameraLink"; // Constants used for Profile persistence
        internal static string comPortDefault = "0";
        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "true";

        internal static string comPort; // Variables to hold the current device configuration

        internal CameraStates cameraState = CameraStates.cameraIdle;
        internal Boolean waiting = false;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;


        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        private CircularAcquisition CirAcq;
        volatile bool WaitFrameThreadContinue;
        private Thread WaitFrameThread;

        private CLAllSerial m_clserial;
        volatile bool m_readThreadContinue;
        private Thread m_readThread;

        private string serialResponse;
        private uint exposurePeriod = (uint)(Math.Pow(2, 32) - 1);


        /// <summary>
        /// Initializes a new instance of the <see cref="PIRT1280SciCam2"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Camera()
        {
            tl = new TraceLogger("", "PIRT1280SciCam2");
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("Camera", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); // Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro-utilities object

            // Implement your additional construction here

            CirAcq = new CircularAcquisition();
            m_clserial = new CLAllSerial();

            cameraState = CameraStates.cameraIdle;

            tl.LogMessage("Camera", "Completed initialisation");

        }


        //
        // PUBLIC COM INTERFACE ICameraV3 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            //using (SetupDialogForm F = new SetupDialogForm(tl))
            //{
            //    var result = F.ShowDialog();
            //    if (result == System.Windows.Forms.DialogResult.OK)
            //    {
            //        WriteProfile(); // Persist device configuration values to the ASCOM Profile store
            //    }
            //}
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            // string retString = CommandString(command, raw); // Send the command and wait for the response
            // bool retBool = XXXXXXXXXXXXX; // Parse the returned string and create a boolean True / False value
            // return retBool; // Return the boolean value to the client

            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            Console.WriteLine("DISPOSING");
            // Clean up the trace logger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;


            ReadThreadStop();
            m_clserial.SerialClose();

            WaitFrameThreadContinue = false;
            WaitFrameThread.Join();
            CirAcq.Cleanup();
            CirAcq.Close();
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    connectedState = true;
                    LogMessage("Connected Set", "Connecting to port {0}", comPort);

                    try
                    {
                        Console.WriteLine("CONNECTING");

                        // Starting CirAcq
                        CirAcqInit(0);

                        // Starting Serial Port
                        SerialPortInit(0);
                        SerialWrite("COMM:ECHO OFF");
                        SerialWrite("CORR:OFFSET OFF");
                        SerialWrite("CORR:GAIN OFF");
                        SerialWrite("CORR:SUB OFF");
                        SerialWrite("DATA:FORMAT 14BIT_BASE");

                    }
                    catch (System.Exception Ex)
                    {
                        Console.WriteLine(Ex.Message);
                    }


                }
                else
                {
                    connectedState = false;
                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);

                    ReadThreadStop();
                    m_clserial.SerialClose();

                    WaitFrameThreadContinue = false;
                    WaitFrameThread.Join();
                    CirAcq.Cleanup();
                    CirAcq.Close();

                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "PIRT1280SciCam2";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ICamera Implementation

        private const int ccdWidth = 1024; // Constants to define the CCD pixel dimensions
        private const int ccdHeight = 1280;
        private const double pixelSize = 12; // Constant for the pixel physical dimension

        private int cameraNumX = ccdWidth; // Initialise variables to hold values required for functionality tested by Conform
        private int cameraNumY = ccdHeight;
        private int cameraStartX = 0;
        private int cameraStartY = 0;
        private DateTime exposureStart = DateTime.MinValue;
        private DateTime exposureRequestedStart = DateTime.MinValue;
        private double cameraLastExposureDuration = 0.0;
        private int newDurationCount = 0;
        private bool cameraImageReady = false;
        private int[,] cameraImageArray;
        private object[,] cameraImageArrayVariant;

        public void AbortExposure()
        {
            tl.LogMessage("AbortExposure", "");
            //throw new MethodNotImplementedException("AbortExposure");
            SetExposure(0.3141); // set random short exposure to allow next exposure to be set faster since no abort function present
            waiting = true;
            cameraState = CameraStates.cameraWaiting;
            newDurationCount = 0;
            cameraLastExposureDuration = 0.3141;
        }

        public short BayerOffsetX
        {
            get
            {
                tl.LogMessage("BayerOffsetX Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("BayerOffsetX", false);
            }
        }

        public short BayerOffsetY
        {
            get
            {
                tl.LogMessage("BayerOffsetY Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("BayerOffsetX", true);
            }
        }

        public short BinX
        {
            get
            {
                tl.LogMessage("BinX Get", "1");
                return 1;
            }
            set
            {
                tl.LogMessage("BinX Set", value.ToString());
                if (value != 1) throw new ASCOM.InvalidValueException("BinX", value.ToString(), "1"); // Only 1 is valid in this simple template
            }
        }

        public short BinY
        {
            get
            {
                tl.LogMessage("BinY Get", "1");
                return 1;
            }
            set
            {
                tl.LogMessage("BinY Set", value.ToString());
                if (value != 1) throw new ASCOM.InvalidValueException("BinY", value.ToString(), "1"); // Only 1 is valid in this simple template
            }
        }

        public double CCDTemperature
        {
            get
            {
                tl.LogMessage("CCDTemperature Get", "1");
                //throw new ASCOM.PropertyNotImplementedException("CCDTemperature", false);

                return Convert.ToDouble(SerialWrite("TEMP:SENS?", true));
            }
        }

        public CameraStates CameraState
        {
            get
            {
                tl.LogMessage("CameraState Get", cameraState.ToString());
                return cameraState;
            }
        }

        public int CameraXSize
        {
            get
            {
                tl.LogMessage("CameraXSize Get", ccdWidth.ToString());
                return ccdWidth;
            }
        }

        public int CameraYSize
        {
            get
            {
                tl.LogMessage("CameraYSize Get", ccdHeight.ToString());
                return ccdHeight;
            }
        }

        public bool CanAbortExposure
        {
            get
            {
                tl.LogMessage("CanAbortExposure Get", false.ToString());
                return false;
            }
        }

        public bool CanAsymmetricBin
        {
            get
            {
                tl.LogMessage("CanAsymmetricBin Get", false.ToString());
                return false;
            }
        }

        public bool CanFastReadout
        {
            get
            {
                tl.LogMessage("CanFastReadout Get", false.ToString());
                return false;
            }
        }

        public bool CanGetCoolerPower
        {
            get
            {
                tl.LogMessage("CanGetCoolerPower Get", false.ToString());
                return false;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                tl.LogMessage("CanPulseGuide Get", false.ToString());
                return false;
            }
        }

        public bool CanSetCCDTemperature
        {
            get
            {
                tl.LogMessage("CanSetCCDTemperature Get", true.ToString());
                return true;
            }
        }

        public bool CanStopExposure
        {
            get
            {
                tl.LogMessage("CanStopExposure Get", false.ToString());
                return false;
            }
        }

        public bool CoolerOn
        {
            get
            {
                tl.LogMessage("CoolerOn Get", "");
                //throw new ASCOM.PropertyNotImplementedException("CoolerOn", false);
                return Convert.ToBoolean(double.Parse(SerialWrite("TEC:EN?", true)));
            }
            set
            {
                tl.LogMessage("CoolerOn Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("CoolerOn", true);
                return;
            }
        }

        public double CoolerPower
        {
            get
            {
                tl.LogMessage("CoolerPower Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("CoolerPower", false);
            }
        }

        public double ElectronsPerADU
        {
            get
            {
                tl.LogMessage("ElectronsPerADU Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ElectronsPerADU", false);
            }
        }

        public double ExposureMax
        {
            get
            {
                tl.LogMessage("ExposureMax Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ExposureMax", false);
            }
        }

        public double ExposureMin
        {
            get
            {
                tl.LogMessage("ExposureMin Get", "");
                //throw new ASCOM.PropertyNotImplementedException("ExposureMin", false);
                return exposureMinVal;
            }
        }

        public double ExposureResolution
        {
            get
            {
                tl.LogMessage("ExposureResolution Get", "");
                //throw new ASCOM.PropertyNotImplementedException("ExposureResolution", false);
                return 1 / 15e6;
            }
        }

        public bool FastReadout
        {
            get
            {
                tl.LogMessage("FastReadout Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FastReadout", false);
            }
            set
            {
                tl.LogMessage("FastReadout Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FastReadout", true);
            }
        }

        public double FullWellCapacity
        {
            get
            {
                tl.LogMessage("FullWellCapacity Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FullWellCapacity", false);
            }
        }

        public short Gain
        {
            get
            {
                tl.LogMessage("Gain Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Gain", false);
            }
            set
            {
                tl.LogMessage("Gain Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Gain", true);
            }
        }

        public short GainMax
        {
            get
            {
                tl.LogMessage("GainMax Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GainMax", false);
            }
        }

        public short GainMin
        {
            get
            {
                tl.LogMessage("GainMin Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GainMin", true);
            }
        }

        public ArrayList Gains
        {
            get
            {
                tl.LogMessage("Gains Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Gains", true);
            }
        }

        public bool HasShutter
        {
            get
            {
                tl.LogMessage("HasShutter Get", false.ToString());
                return false;
            }
        }

        public double HeatSinkTemperature
        {
            get
            {
                //tl.LogMessage("HeatSinkTemperature Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("HeatSinkTemperature", false);
                //return Convert.ToDouble(SerialWrite("TEMP:CASE?", true));
            }
        }

        public object ImageArray
        {
            get
            {
                if (!cameraImageReady)
                {
                    tl.LogMessage("ImageArray Get", "Throwing InvalidOperationException because of a call to ImageArray before the first image has been taken!");
                    throw new ASCOM.InvalidOperationException("Call to ImageArray before the first image has been taken!");
                }
                tl.LogMessage("ImageArray Get", "");

                return cameraImageArray;
            }
        }

        public object ImageArrayVariant
        {
            get
            {
                if (!cameraImageReady)
                {
                    tl.LogMessage("ImageArrayVariant Get", "Throwing InvalidOperationException because of a call to ImageArrayVariant before the first image has been taken!");
                    throw new ASCOM.InvalidOperationException("Call to ImageArrayVariant before the first image has been taken!");
                }
                cameraImageArrayVariant = new object[cameraNumX, cameraNumY];
                for (int i = 0; i < cameraImageArray.GetLength(1); i++)
                {
                    for (int j = 0; j < cameraImageArray.GetLength(0); j++)
                    {
                        cameraImageArrayVariant[j, i] = cameraImageArray[j, i];
                    }

                }

                return cameraImageArrayVariant;
            }
        }

        public bool ImageReady
        {
            get
            {
                tl.LogMessage("ImageReady Get", cameraImageReady.ToString());

                // REMOVE FIRST IF NOT USING ACP
                // if (cameraImageReady && (DateTime.Now - exposureRequestedStart).TotalSeconds < 1.5)
                // {
                //     tl.LogMessage("ImageReady Get", "Less than 1s");

                //     return false;
                // } 
                if (cameraImageReady && waiting)
                {
                    tl.LogMessage("ImageReady Get", "Camera waiting ffs");
                    return false;
                }
                else
                {
                    return cameraImageReady;
                }
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                tl.LogMessage("IsPulseGuiding Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("IsPulseGuiding", false);
            }
        }

        public double LastExposureDuration
        {
            get
            {
                if (!cameraImageReady)
                {
                    tl.LogMessage("LastExposureDuration Get", "Throwing InvalidOperationException because of a call to LastExposureDuration before the first image has been taken!");
                    throw new ASCOM.InvalidOperationException("Call to LastExposureDuration before the first image has been taken!");
                }
                tl.LogMessage("LastExposureDuration Get", cameraLastExposureDuration.ToString());
                return cameraLastExposureDuration;
            }
        }

        public string LastExposureStartTime
        {
            get
            {
                if (!cameraImageReady)
                {
                    tl.LogMessage("LastExposureStartTime Get", "Throwing InvalidOperationException because of a call to LastExposureStartTime before the first image has been taken!");
                    throw new ASCOM.InvalidOperationException("Call to LastExposureStartTime before the first image has been taken!");
                }
                string exposureStartString = exposureStart.ToString("yyyy-MM-ddTHH:mm:ss");
                tl.LogMessage("LastExposureStartTime Get", exposureStartString.ToString());
                return exposureStartString;
            }
        }

        public int MaxADU
        {
            get
            {
                tl.LogMessage("MaxADU Get", "");
                return (int)(Math.Pow(2, 14) - 1);
            }
        }

        public short MaxBinX
        {
            get
            {
                tl.LogMessage("MaxBinX Get", "1");
                return 1;
            }
        }

        public short MaxBinY
        {
            get
            {
                tl.LogMessage("MaxBinY Get", "1");
                return 1;
            }
        }

        public int NumX
        {
            get
            {
                tl.LogMessage("NumX Get", cameraNumX.ToString());
                return cameraNumX;
            }
            set
            {
                cameraNumX = value;
                tl.LogMessage("NumX set", value.ToString());
            }
        }

        public int NumY
        {
            get
            {
                tl.LogMessage("NumY Get", cameraNumY.ToString());
                return cameraNumY;
            }
            set
            {
                cameraNumY = value;
                tl.LogMessage("NumY set", value.ToString());
            }
        }

        public int Offset
        {
            get
            {
                tl.LogMessage("Offset Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Offset", false);
            }
            set
            {
                tl.LogMessage("Offset Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Offset", true);
            }
        }

        public int OffsetMax
        {
            get
            {
                tl.LogMessage("OffsetMax Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("OffsetMax", false);
            }
        }

        public int OffsetMin
        {
            get
            {
                tl.LogMessage("OffsetMin Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("OffsetMin", true);
            }
        }

        public ArrayList Offsets
        {
            get
            {
                tl.LogMessage("Offsets Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Offsets", true);
            }
        }

        public short PercentCompleted
        {
            get
            {
                tl.LogMessage("PercentCompleted Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("PercentCompleted", false);
            }
        }

        public double PixelSizeX
        {
            get
            {
                tl.LogMessage("PixelSizeX Get", pixelSize.ToString());
                return pixelSize;
            }
        }

        public double PixelSizeY
        {
            get
            {
                tl.LogMessage("PixelSizeY Get", pixelSize.ToString());
                return pixelSize;
            }
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            tl.LogMessage("PulseGuide", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("PulseGuide");
        }

        public short ReadoutMode
        {
            get
            {
                tl.LogMessage("ReadoutMode Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ReadoutMode", false);
                //return 0;
            }
            set
            {
                tl.LogMessage("ReadoutMode Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("ReadoutMode", true);
                //return 0;
            }
        }

        public ArrayList ReadoutModes
        {
            get
            {
                tl.LogMessage("ReadoutModes Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ReadoutModes", false);
            }
        }

        public string SensorName
        {
            get
            {
                tl.LogMessage("SensorName Get", "");
                //throw new ASCOM.PropertyNotImplementedException("SensorName", false);
                return SerialWrite("SYS:SN?");
            }
        }

        public SensorType SensorType
        {
            get
            {
                tl.LogMessage("SensorType Get", "");
                //throw new ASCOM.PropertyNotImplementedException("SensorType", false);
                return 0;
            }
        }

        public double SetCCDTemperature
        {
            get
            {
                tl.LogMessage("SetCCDTemperature Get", "");
                //throw new ASCOM.PropertyNotImplementedException("SetCCDTemperature", false);
                return Convert.ToDouble(SerialWrite("TEMP:SENS:SET?", true));
            }
            set
            {
                tl.LogMessage("SetCCDTemperature Set", "");
                //throw new ASCOM.PropertyNotImplementedException("SetCCDTemperature", true);
                SerialWrite("TEMP:SENS:SET " + value.ToString());
                return;
            }
        }

        public void StartExposure(double Duration, bool Light)
        {
            if (Duration < 0.0) throw new InvalidValueException("StartExposure", Duration.ToString(), "0.0 upwards");
            if (cameraNumX > ccdWidth) throw new InvalidValueException("StartExposure", cameraNumX.ToString(), ccdWidth.ToString());
            if (cameraNumY > ccdHeight) throw new InvalidValueException("StartExposure", cameraNumY.ToString(), ccdHeight.ToString());
            if (cameraStartX > ccdWidth) throw new InvalidValueException("StartExposure", cameraStartX.ToString(), ccdWidth.ToString());
            if (cameraStartY > ccdHeight) throw new InvalidValueException("StartExposure", cameraStartY.ToString(), ccdHeight.ToString());

            tl.LogMessage("StartExposure", "begin");
            exposureRequestedStart = DateTime.Now;
            cameraImageReady = false;

            if (Duration == 0)
            {
                Duration = exposureMinVal;
            }

            if (cameraLastExposureDuration != Duration)
            {
                SetExposure(Duration);
                waiting = true;
                cameraState = CameraStates.cameraWaiting;
                newDurationCount = 0;
            }
            else
            {
                TakeImage();
            }
            cameraLastExposureDuration = Duration;
            tl.LogMessage("StartExposure", Duration.ToString() + " " + Light.ToString());
        }

        public int StartX
        {
            get
            {
                tl.LogMessage("StartX Get", cameraStartX.ToString());
                return cameraStartX;
            }
            set
            {
                cameraStartX = value;
                tl.LogMessage("StartX Set", value.ToString());
            }
        }

        public int StartY
        {
            get
            {
                tl.LogMessage("StartY Get", cameraStartY.ToString());
                return cameraStartY;
            }
            set
            {
                cameraStartY = value;
                tl.LogMessage("StartY set", value.ToString());
            }
        }

        public void StopExposure()
        {
            tl.LogMessage("StopExposure", "Not implemented");
            throw new MethodNotImplementedException("StopExposure");
        }

        public double SubExposureDuration
        {
            get
            {
                tl.LogMessage("SubExposureDuration Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SubExposureDuration", false);
            }
            set
            {
                tl.LogMessage("SubExposureDuration Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SubExposureDuration", true);
            }
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Camera";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Camera";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Camera";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }

        private void WaitForFrameThread()
        {
            BufferInfo BufInfo = new BufferInfo();
            WaitFrameDoneReturns ReturnValue;
            byte[] imageBuffer = new byte[1024 * 1280 * 2];
            // int[,] image;

            Console.WriteLine("Thread called");

            while (WaitFrameThreadContinue)
            {
                try
                {
                    //if (cameraImageReady == false)
                    if (true)
                    {
                        //Console.WriteLine("Acq");
                        if ((ReturnValue = CirAcq.WaitForFrameDone(exposurePeriod, ref BufInfo)) == WaitFrameDoneReturns.FrameAcquired)
                        {
                            //tl.LogMessage("Last_duration", cameraLastExposureDuration.ToString());
                            if (waiting)
                            {
                                newDurationCount += 1;

                                if (newDurationCount >= 2)
                                {
                                    TakeImage();
                                    waiting = false;
                                }
                                tl.LogMessage("waiting", newDurationCount.ToString());

                                imageBuffer = CirAcq.GetBufferData(BufInfo.m_BufferNumber);
                                CirAcq.SetBufferStatus(BufInfo.m_BufferNumber, BufferStatus.Available);
                            }
                            else if (!waiting)
                            {
                                //cameraState = CameraStates.cameraReading;

                                imageBuffer = CirAcq.GetBufferData(BufInfo.m_BufferNumber);

                                //tl.LogMessage("Image Acq time", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                                //tl.LogMessage("bufnum", BufInfo.m_BufferNumber.ToString());

                                exposureStart = DateTime.Now.AddSeconds(-1 * cameraLastExposureDuration - 100e-3);
                                cameraImageArray = ConvertArray(imageBuffer, 1280);

                                cameraImageReady = true;
                                cameraState = CameraStates.cameraIdle;

                                // After "processing" the buffer, mark it available for new image data.
                                CirAcq.SetBufferStatus(BufInfo.m_BufferNumber, BufferStatus.Available);
                            }

                        }
                        else if (ReturnValue == WaitFrameDoneReturns.AcqusitionStopped)
                        {
                            Console.WriteLine("Acquisition was stopped.");
                            WaitFrameThreadContinue = false;
                        }
                        else if (ReturnValue == WaitFrameDoneReturns.AcqusitionAborted)
                        {
                            Console.WriteLine("Acquisition was aborted.");
                            WaitFrameThreadContinue = false;
                        }
                        else if (ReturnValue == WaitFrameDoneReturns.CleanupCalled)
                        {
                            Console.WriteLine("Cleanup was called, ending acquisition.");
                            WaitFrameThreadContinue = false;
                        }
                        else
                        {
                            Console.WriteLine("Unknown return value from WaitForFrameDone.");
                            WaitFrameThreadContinue = false;
                        }
                    }
                }
                catch (System.Exception Ex)
                {
                    Console.WriteLine(Ex.Message);
                    WaitFrameThreadContinue = false;
                    throw new ASCOM.InvalidOperationException(Ex.Message);
                }
            }

            // Make sure we are not acquiring images
            CirAcq.AbortAcquisition(AcqControlOptions.Wait);
            Console.WriteLine("ending thread");

            return;
        }


        private int[,] ConvertArray(byte[] Input, int size)
        {
            int[,] Output = new int[(int)(Input.Length / (2 * size)), size];
            byte hiByte;
            byte lowByte;
            int pixel;


            for (int i = 0; i < Input.Length; i += size * 2)
            {
                for (int j = 0; j < size * 2; j++)
                {
                    if (j % 2 == 0)
                    {
                        hiByte = Input[i + j + 1];
                        lowByte = Input[i + j];
                        pixel = (int)((hiByte << 8) | lowByte);
                        Output[(int)(i / (size * 2)), j / 2] = pixel;
                    }

                }
            }

            if (cameraNumX != ccdWidth && cameraNumY != ccdHeight)
            {

                int[,] OutputSubFrame = new int[cameraNumX, cameraNumY];

                for (int i = cameraStartX; i < cameraStartX + cameraNumX; i++)
                {
                    for (int j = cameraStartY; j < cameraStartY + cameraNumY; j++)
                    {
                        OutputSubFrame[i - cameraStartX, j - cameraStartY] = Output[i, j];
                    }
                }

                return OutputSubFrame;

            }
            else
            {
                return Output;
            }

        }

        private void CirAcqInit(UInt32 index)
        {
            // Open board, this must be the first method called after
            // instantiation of CircularAcquisition.
            CirAcq.Open(index);

            // Stop acquisition if the software can't keep up with the board.
            CirAcq.SetOverwriteMethod(OverwriteMethod.Ignore);

            // Use defalut setup options
            CirAcq.SetSetupOptions(SetupOptions.Default);

            // Setup for circular acquisition
            CirAcq.Setup(2);

            // The method will return immediately after issuing a start to the acquisition engine. 
            CirAcq.StartAcquisition(AcqControlOptions.Wait);

            // Clears the buffers's contents by writing zeros to all buffers. Setup must be called before calling this method.
            CirAcq.ClearBuffers();

            // Create a thread that waits for frames to be acquired then
            // display the pixel value of the first pixel.
            WaitFrameThread = new Thread(new ThreadStart(WaitForFrameThread));
            WaitFrameThread.Name = "CirAcq";
            WaitFrameThreadContinue = true;
            WaitFrameThread.Start();
        }

        private void SerialPortInit(UInt32 index)
        {
            try
            {
                // Stop the data receive thread.
                ReadThreadStop();

                // Close the opened serial port.
                m_clserial.SerialClose();

                // Attempt to open the designated port.
                m_clserial.SerialInit(index);

                //string manufacturer = "", portID = "";
                //uint version = 0;
                //CLAllSerial.GetPortInfo(index, ref manufacturer, ref portID, ref version);
                //Console.WriteLine(manufacturer + ", " + portID + ", " + version);

                m_clserial.BFSerialSettings(CLAllSerial.BaudRates.CL_BAUDRATE_115200,
                                            CLAllSerial.DataBits.DataBits_8,
                                            CLAllSerial.Parity.ParityNone,
                                            CLAllSerial.StopBits.StopBits_1);

                // Restart the data receive thread.
                m_readThread = new Thread(new ThreadStart(SerialReadThreadFunc));
                m_readThread.Name = "m_clserial";
                m_readThreadContinue = true;
                m_readThread.Start();

            }
            catch (ApplicationException err)
            {
                Console.WriteLine("Serial port initialization failed: " + err.Message, "CLSerial Initialization Error");
            }
        }


        private void SerialReadThreadFunc()
        {
            string Rx = "";
            string LatestRx;

            try
            {
                // This is a BF port, so prefer the BFSerialRead method.
                while (m_readThreadContinue)
                {
                    LatestRx = m_clserial.BFSerialRead(8);
                    LatestRx = LatestRx.Replace("\r\n", "\r").Replace("\n", "").Replace("\0", "").Replace(">", "");

                    Rx = string.Concat(Rx, LatestRx);

                    if (LatestRx.Contains("\r"))
                    {
                        //Console.WriteLine(Rx);
                        if (Rx != "OK\r")
                        {
                            serialResponse = Rx;
                        }
                        Rx = "";
                    }
                }
            }
            catch (System.Exception err)
            {
                Console.WriteLine("Caught an exception: " + err.Message + "\n\nAborting the read thread. Data can still be written, but no data will be received.", "Data Read Thread Error");
            }

            return;
        }

        private void ReadThreadStop()
        {
            if (null == m_readThread)
                return;

            do
            {
                m_readThreadContinue = false;
                if (m_clserial.PortIsBF)
                    m_clserial.BFSerialCancelRead();
            }
            while (!m_readThread.Join(10));
        }

        private string SerialWrite(string enteredText, bool numeric = false)
        {
            serialResponse = "";
            string r = "";
            bool isNumeric = false;
            // Write the text, and append it to the list.
            string writeText = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(enteredText + "\r"));
            tl.LogMessage("SerialWrite", writeText);
            try
            {
                do
                {
                    tl.LogMessage("SerialWrite", "in do");
                    m_clserial.SerialWrite(writeText, 1000);

                    SpinWait.SpinUntil(() =>
                    {
                        if (serialResponse != "" || serialResponse.EndsWith("K\r"))
                        {
                            r = serialResponse;
                            tl.LogMessage("SerialResponse", r);
                            //Console.WriteLine(r);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }, TimeSpan.FromSeconds(1));

                    r = r.Trim();

                    if (r == "ON")
                    {
                        r = "1";
                    }
                    else if (r == "OFF")
                    {
                        r = "0";
                    }

                    isNumeric = double.TryParse(r, out _);
                    tl.LogMessage("r", r);
                    tl.LogMessage("isNumeric", isNumeric.ToString());


                } while (!isNumeric && numeric);

            }
            catch (ApplicationException err)
            {
                Console.WriteLine(err.Message, "CLSerial Write Error");
            }

            return r;
        }

        private void SetExposure(double exposureTime)
        {
            tl.LogMessage("SetExposure", exposureTime.ToString());
            double clockFreq = 15e6;
            long exposureCycles = (long)(clockFreq * exposureTime);

            double frameTime = 100e-3; // 100ms added to the exposure time
            long frameCycles = exposureCycles + (long)(clockFreq * frameTime);



            SerialWrite("SENS:FRAMEPER " + frameCycles.ToString());
            SerialWrite("SENS:EXPPER " + exposureCycles.ToString());
            SerialWrite("SENS:FRAMEPER " + frameCycles.ToString());
            SerialWrite("SENS:EXPPER " + exposureCycles.ToString());
        }

        private void TakeImage()
        {
            tl.LogMessage("TakeImage", "");

            cameraImageReady = false;
            cameraState = CameraStates.cameraExposing;
        }

        #endregion
    }
}
