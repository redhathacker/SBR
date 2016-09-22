using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Lextm.SharpSnmpLib;
using System.Linq;
using System.Collections.Concurrent; // USE THESE COLLECTIONS WHEN DEALING WITH MUTIPLE THREADS

// arrays and lists are Ienumerable objects - 
// Get Index - ((IList<Channel>)__contents).IndexOf(value);
// Conv Array to Ienum -  IEnumerable<Channel> __RandomCh = __contents; 
// Convert List to Ienumerable IEnumerable<Channel> __ChannelColl = __lstChannels;


namespace snmpd
{
    /// <summary>
    /// Implementzations: IClonable (MemberWise), ICollection, IEnumerator, IEmumeration, IEquatable
    /// This class can compare itself to another copy to see if they are equal.Equal() IEquatable interface
    /// This class can make new instaces that are exact compies of itself.IClonable interface
    /// Allows you to set the value of the channel </summary>

    // DONE DONT TOUCH
    #region Class: Channel Definition

    public class Channel : IEquatable<Channel>, ICloneable
    {
        // Public Properties
        public ChannelState State { get; set; }
        public int Id { get; protected set; }
        public string Name { get; protected set; }

        /// <summary>
        /// This ObjectIdentifer holds the unique OID value of the channel.
        /// </summary>
        public ObjectIdentifier OID { get; protected set; }

        /// <summary>
        /// Creates and empty Channel object.
        /// </summary>
        public Channel()
        {
            State = ChannelState.OFF;
        }

        // Channel Constructors, There is no empty constructor
        /// <summary>
        /// Create a new analog channel.
        /// </summary>
        public Channel(int _id, string _oid, string _name, int _value)
        {
            Id = _id;
            OID = new ObjectIdentifier(_oid);
            Name = _name;
        }

        public Channel(int _id, string _oid, string _name, ChannelState _state)
        {
            Id = _id;
            OID = new ObjectIdentifier(_oid);
            Name = _name;
            State = _state;
        }
        public Channel(int _id, ObjectIdentifier _oid, string _name, ChannelState _state)
        {
            Id = _id;
            OID = _oid;
            Name = _name;
            State = _state;
        }

        /// <summary>
        /// Change Value (Digital only)
        /// </summary>
        public void ToggleState()
        {
            State = (State == ChannelState.ON ? ChannelState.OFF : ChannelState.ON);
        }

        public void SetState(ChannelState pSt)
        {
            State = pSt;
        }

        /// <summary>
        /// Is some other channel identical to this one. 
        /// </summary>
        /// <param name="other">The other channel.</param>
        bool IEquatable<Channel>.Equals(Channel other)
        {
            if (OID == other.OID && Name == other.Name && Id == other.Id)
                return true;

            return false;
        }

        #region ICloneable Definitions

        /// <summary>
        /// Create a brand new Channel instance that is a copy of this one. 
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            // Ha!! Turns out my code is better. Makes a true copy.
            // return new Channel(Id, OID, Name, State);
            // Microsofts method is to use MemberWise
            return DeepCopy();
        }

        /// <summary>
        /// Basically just copies the memory address. The copy will change values in the original variable;
        /// </summary>
        /// <returns></returns>
        private Channel ShallowCopy()
        {
            return (Channel)MemberwiseClone();
        }
        /// <summary>
        /// Copies everything structure and values into a new brand new memory location;
        /// </summary>
        /// <returns></returns>
        private Channel DeepCopy()
        {
            Channel other = (Channel)MemberwiseClone();
            other.OID = new ObjectIdentifier(this.OID.ToString());
            other.Name = String.Copy(Name);
            other.State = this.State;
            other.Id = this.Id;
            return other;
        }
    }

    #endregion


    #endregion Class: Channel

    /// <summary>
    /// An object that represents an analog channel.
    /// </summary>
    public class AnalogChannel : Channel
    {
        /// <summary>
        /// The integer value of the channel (0-1024).
        /// </summary>
        public int Value { get; protected set; }

        /// <summary>
        ///  The new value of this channel (0-1024).
        /// </summary>
        public bool SetValue(int pNewAnalogValue)
        {
            bool success = false;

            if (pNewAnalogValue > 0 || pNewAnalogValue <= 1024)
            {
                Value = pNewAnalogValue;
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Create a new analog channel.
        /// </summary>
        public AnalogChannel(int _id, string _oid, string _name, int _value)
        {
            this.Id = _id;
            OID = new ObjectIdentifier(_oid);
            Name = _name;
            SetValue(_value);
        }

        /// <summary>
        ///  Create a new analog channel.
        /// </summary>
        /// <param name="_id">The Channel ID.</param>
        /// <param name="_oid">The OID of the channel; Object.</param>
        /// <param name="_name">Channel name.</param>
        /// <param name="_value">Channel value (0 - 1024).</param>
        public AnalogChannel(int _id, ObjectIdentifier _oid, string _name, int _value)
        {
            Id = _id;
            OID = _oid;
            Name = _name;
            SetValue(_value);
        }

    }


    #region ChannelHelper class

    /// <summary>
    /// This class is used to manage access to the channels. 
    /// </summary>
    public static class ChannelHelper
    {
        public static readonly ObjectIdentifier Io1_Ch1 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.1.0");
        public static readonly ObjectIdentifier Io1_Ch2 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.2.0");
        public static readonly ObjectIdentifier Io1_Ch3 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.3.0");
        public static readonly ObjectIdentifier Io1_Ch4 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.4.0");
        public static readonly ObjectIdentifier Io1_Ch5 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.5.0");
        public static readonly ObjectIdentifier Io1_Ch6 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.6.0");
        public static readonly ObjectIdentifier Io1_Ch7 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.7.0");
        public static readonly ObjectIdentifier Io1_Ch8 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.8.0");
        public static readonly ObjectIdentifier Io1_All = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.33.0");

        public static readonly ObjectIdentifier Io2_Ch1 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.1.0");
        public static readonly ObjectIdentifier Io2_Ch2 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.2.0");
        public static readonly ObjectIdentifier Io2_Ch3 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.3.0");
        public static readonly ObjectIdentifier Io2_Ch4 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.4.0");
        public static readonly ObjectIdentifier Io2_Ch5 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.5.0");
        public static readonly ObjectIdentifier Io2_Ch6 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.6.0");
        public static readonly ObjectIdentifier Io2_Ch7 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.7.0");
        public static readonly ObjectIdentifier Io2_Ch8 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.8.0");
        public static readonly ObjectIdentifier Io2_All = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.33.0");

        public static readonly ObjectIdentifier Ad1_Ch1 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.1.0");
        public static readonly ObjectIdentifier Ad1_Ch2 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.2.0");
        public static readonly ObjectIdentifier Ad1_Ch3 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.4.3.0");
        public static readonly ObjectIdentifier Ad1_Ch4 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.4.0");
        public static readonly ObjectIdentifier Ad1_Ch5 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.5.0");
        public static readonly ObjectIdentifier Ad1_Ch6 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.6.0");
        public static readonly ObjectIdentifier Ad1_Ch7 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.7.0");
        public static readonly ObjectIdentifier Ad1_Ch8 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.8.0");
        public static readonly ObjectIdentifier Ad1_All = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.33.0");

        public static Channel ch1_1 = new Channel(1, Io1_Ch1, "Chan1_Io1", 0);
        public static Channel ch1_2 = new Channel(2, Io1_Ch2, "Chan2_Io1", 0);
        public static Channel ch1_3 = new Channel(3, Io1_Ch3, "Chan3_Io1", 0);
        public static Channel ch1_4 = new Channel(4, Io1_Ch4, "Chan4_Io1", 0);
        public static Channel ch1_5 = new Channel(5, Io1_Ch5, "Chan5_Io1", 0);
        public static Channel ch1_6 = new Channel(6, Io1_Ch6, "Chan6_Io1", 0);
        public static Channel ch1_7 = new Channel(7, Io1_Ch7, "Chan7_Io1", 0);
        public static Channel ch1_8 = new Channel(8, Io1_Ch8, "Chan8_Io1", 0);
        public static Channel ch1_33 = new Channel(33, Io1_All, "ChanAll_Io1", 0);

        public static Channel ch2_1 = new Channel(1, Io2_Ch1, "Chan1_Io2", 0);
        public static Channel ch2_2 = new Channel(2, Io2_Ch2, "Chan2_Io2", 0);
        public static Channel ch2_3 = new Channel(3, Io2_Ch3, "Chan3_Io2", 0);
        public static Channel ch2_4 = new Channel(4, Io2_Ch4, "Chan4_Io2", 0);
        public static Channel ch2_5 = new Channel(5, Io2_Ch5, "Chan5_Io2", 0);
        public static Channel ch2_6 = new Channel(6, Io2_Ch6, "Chan6_Io2", 0);
        public static Channel ch2_7 = new Channel(7, Io2_Ch7, "Chan7_Io2", 0);
        public static Channel ch2_8 = new Channel(8, Io2_Ch8, "Chan8_Io2", 0);
        public static Channel ch2_33 = new Channel(33, Io2_All, "ChanAll_Io2", 0);

        public static Channel ch3_1 = new Channel(1, Ad1_Ch1, "Chan1_Ad1", 0);
        public static Channel ch3_2 = new Channel(2, Ad1_Ch2, "Chan2_Ad1", 0);
        public static Channel ch3_3 = new Channel(3, Ad1_Ch3, "Chan3_Ad1", 0);
        public static Channel ch3_4 = new Channel(4, Ad1_Ch4, "Chan4_Ad1", 0);
        public static Channel ch3_5 = new Channel(5, Ad1_Ch1, "Chan5_Ad1", 0);
        public static Channel ch3_6 = new Channel(6, Ad1_Ch2, "Chan6_Ad1", 0);
        public static Channel ch3_7 = new Channel(7, Ad1_Ch3, "Chan7_Ad1", 0);
        public static Channel ch3_8 = new Channel(8, Ad1_Ch4, "Chan8_Ad1", 0);
        public static Channel ch3_33 = new Channel(33, Ad1_All, "ChanAll_Ad1", 0);

        public static readonly List<Channel> AllChannels = new List<Channel>
          { ch1_1, ch1_2, ch1_3, ch1_4, ch1_5, ch1_6, ch1_7, ch1_8,
          ch2_1, ch2_2, ch2_3, ch2_4, ch2_5, ch2_6, ch2_7, ch2_8,
          ch3_1, ch3_2, ch3_3, ch3_4 }; //, ch3_5, ch3_6, ch3_7, ch3_8 };

        public static readonly List<Channel> AllDigitalBoard1Channels = new List<Channel>
          { ch1_1, ch1_2, ch1_3, ch1_4, ch1_5, ch1_6, ch1_7, ch1_8 };

        public static readonly List<Channel> AllDigitalBoard2Channels = new List<Channel>
          { ch2_1, ch2_2, ch2_3, ch2_4, ch2_5, ch2_6, ch2_7, ch2_8 };

        public static readonly List<Channel> AllAnalogBoard1Channels = new List<Channel>
          { ch3_1, ch3_2, ch3_3, ch3_4 };

        private const int TotalNumberOfChannels = 20;

        // public properties
        //public List<Channel> DigitalChannels { get { return _lstChannels; } }
        //public Channel Current
        //{
        //     get
        //     {
        //          try
        //          {
        //               return _lstChannels[CurrentIndex];
        //          }
        //          catch (IndexOutOfRangeException)
        //          {
        //               throw new InvalidOperationException();
        //          }
        //     }
        //}

        //#region Controller Type methods (Add, Get, Set, etc.)

        ///// <summary>
        ///// Returns a Channel object.
        ///// </summary>
        ///// <param name="Oid">The Object Identifier of the channel.</param>
        ///// <returns></returns>
        //public Channel GetChannel(string Oid)
        //{
        //     return _lstChannels.Find(c => c.OID.ToString() == Oid);
        //}

        ///// <summary>
        ///// Returns a Channel object.
        ///// </summary>
        ///// <param name="BoardId">The ID of the Board.</param>
        ///// <param name="ChannelNo">The Channel Number.</param>
        ///// <returns></returns>
        //private Channel GetChannel(EthernetBoardPortNo IoPortNo, int ChannelNo)
        //{
        //     if (IoPortNo == EthernetBoardPortNo.DIGITAL_PORT_1)
        //     {
        //          return _lstDigitalBoard1Channels.Find(c => c.Id == ChannelNo);
        //     }
        //     else if (IoPortNo == EthernetBoardPortNo.ADC_PORT_1)
        //     {
        //          return _lstAnalogBoard1Channels.Find(c => c.Id == ChannelNo);
        //     }
        //     else // Default is Port 2 - most common
        //     {
        //          return _lstDigitalBoard2Channels.Find(c => c.Id == ChannelNo);
        //     }
        //}

        ///// <summary>
        ///// Returns an IEnumerable object that contains all of the channel objects belonging to the specified board.
        ///// </summary>
        ///// <param name="BoardId">The ID of an EthernetBoard.</param>
        //public IEnumerable<Channel> GetBoardChannels(EthernetBoardPortNo IoPortNo)
        //{
        //     if (IoPortNo == EthernetBoardPortNo.DIGITAL_PORT_1)
        //     {
        //          return _lstDigitalBoard1Channels;
        //     }
        //     else if (IoPortNo == EthernetBoardPortNo.ADC_PORT_1)
        //     {
        //          return _lstAnalogBoard1Channels;
        //     }
        //     else // Default is Port 2 - most common
        //     {
        //          return _lstDigitalBoard2Channels;
        //     }
        //}

        ///// <summary>
        ///// Returns an IEnumerable object of Channel objects belonging to the specified board.
        ///// </summary>
        //public IEnumerable<Channel> GetAllBoardChannels()
        //{
        //     return _lstChannels;
        //}

        //#endregion
    }

    #endregion
}

