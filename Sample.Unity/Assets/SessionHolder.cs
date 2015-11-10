using UnityEngine;
using System.Collections;
using Sample.ChatService.Protocol;
using UnityEngine.UI;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using Netbase.Shared;
public class SessionHolder : MonoBehaviour 
{    
    public  Text                ChatOutput;
    public  InputField          InputF;

    private UnityChatSession    m_hChatSession;

    void Awake()
    {
        m_hChatSession = new UnityChatSession(ChatOutput);
        m_hChatSession.Connect("127.0.0.1", 6666);
        m_hChatSession.Login("Unity", "Client");        
    }

    //Thread Unity Main
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(InputF.text))
        {
            m_hChatSession.Message(InputF.text); //c => s => c => 200ms
            InputF.text = string.Empty;
        }

        //m_hChatSession.Messages.ForEach(hM => ChatOutput.text += hM);        
    }
}

public class UnityChatSession : ChatSession
{    
    private List<string> m_hMessages;

    public UnityChatSession(Text hText) 
    {              
        m_hMessages = new List<string>();
    }

    public override void OnForwardMessage(string sSender, string sMessage)
    {
        lock (m_hMessages)
        {
            m_hMessages.Add(sSender + ": " + sMessage + Environment.NewLine);
        }        
    }

    public List<string> Messages
    {
        get 
        {
            lock (m_hMessages)
            {
                List<string> hRes = new List<string>(m_hMessages);
                m_hMessages.Clear();
                return hRes;
            }
        }
    }

    
}



