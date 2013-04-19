using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Diagnostics;

public class UserPrefs
{
    private static XmlTextReader userPrefsFile;
    public Hashtable userPrefs;
 
    public UserPrefs(String file)
    {
        userPrefsFile = new XmlTextReader(file);
        userPrefs = new Hashtable();
        String id ="", value="";
        while (userPrefsFile.Read())
        {
            switch (userPrefsFile.NodeType)
            {
                case XmlNodeType.Element:
                    id = userPrefsFile.Name;

                    while (userPrefsFile.MoveToNextAttribute())
                        value = userPrefsFile.Value;
                    break;
            }
            if (id != "i" && value != "v")
           importPreference(id, value);
            id = "i";
            value = "v";
        } 
    }

    private void importPreference(string i, string v){
        if (v.ToLower() == "true")
        {
            userPrefs.Add(i, true);
            return;
        }
        if (v.ToLower() == "false")
        {
            userPrefs.Add(i, false);
            return;
        }
        int x = 0;
        bool isint = int.TryParse(v, out x);
        if (isint)
        {
            userPrefs.Add(i, x);
            return;
        }
        userPrefs[i] = v;
    }

    public bool debug
    {
        get { return (bool)userPrefs["debug"]; }
        set { userPrefs["debug"] = value; }
    }
    public string mediaplayerprocess
    {
        get { return (string)userPrefs["mediaplayerprocess"]; }
        set { userPrefs["mediaplayerprocess"] = value; }
    }
    public int initialvolume
    {
        get { return (int)userPrefs["initialvolume"]; }
        set { userPrefs["initialvolume"] = value; }
    }
    public bool doestoggle
    {
        get { return (bool)userPrefs["doestoggle"]; }
        set { userPrefs["doestoggle"] = value; }
    }
    public bool usegooglevoice
    {
        get { return (bool)userPrefs["usegooglevoice"]; }
        set { userPrefs["usegooglevoice"] = value; }
    }
    public string googleemail
    {
        get { return (string)userPrefs["googleemail"]; }
        set { userPrefs["googleemail"] = value; }
    }
    public string googlepassword
    {
        get { return (string)userPrefs["googlepassword"]; }
        set { userPrefs["googlepassword"] = value; }
    }
    public string googleaddressbook
    {
        get { return (string)userPrefs["googleaddressbook"]; }
        set { userPrefs["googleaddressbook"] = value; }
    }
    public bool usegooglecalendar
    {
        get { return (bool)userPrefs["usegooglecalendar"]; }
        set { userPrefs["usegooglecalendar"] = value; }
    }
    public string facebookrssfeed
    {
        get { return (string)userPrefs["facebookrssfeed"]; }
        set { userPrefs["facebookrssfeed"] = value; }
    }
    public int updateinterval
    {
        get { return (int)userPrefs["updateinterval"]; }
        set { userPrefs["updateinterval"] = value; }
    }
    public int googlecalendaralerttime
    {
        get { return (int)userPrefs["googlecalendaralerttime"]; }
        set { userPrefs["googlecalendaralerttime"] = value; }
    }
    public int volumeincrements
    {
        get { return (int)userPrefs["volumeincrements"]; }
        set { userPrefs["volumeincrements"] = value; }
    }
}
