// frontend/src/context/MessageContext.tsx
import React, { createContext, useContext, useState } from 'react';
import type { Message, MessageFilter } from '../types';
import * as api from '../services/api';

interface MessageContextType {
  messages: Message[];
  loading: boolean;
  error: string | null;
  fetchMessages: (filters?: MessageFilter) => Promise<void>;
  getMessage: (id: string) => Promise<Message>;
  resendMessage: (id: string, options?: any) => Promise<any>;
  batchResend: (messageIds: string[], options?: any) => Promise<any>;
  updateMessageContent: (id: string, content: string) => Promise<any>;
}

const MessageContext = createContext<MessageContextType | null>(null);

export const MessageProvider: React.FC<{children: React.ReactNode}> = ({ children }) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const fetchMessages = async (filters: MessageFilter = {}) => {
    setLoading(true);
    try {
      const response = await api.getMessages(filters);
      
      // Explicitly map the response to ensure type compatibility
      const typedMessages: Message[] = response.map((msg: any) => ({
        id: msg.id || msg.messageId,
        patientId: msg.patientId,
        clinicId: msg.clinicId,
        messageType: msg.messageType,
        status: msg.status,
        timestamp: msg.timestamp || msg.createdAt || new Date().toISOString(),
        hl7Content: msg.hl7Content || msg.convertedHl7Content
      }));
      
      setMessages(typedMessages);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  };
  
  const getMessage = async (id: string): Promise<Message> => {
    try {
      const response = await api.getMessage(id);
      // Map API response to our Message type
      return {
        id: response.id,
        patientId: response.patientId,
        clinicId: response.clinicId,
        messageType: response.messageType,
        status: response.status,
        timestamp: response.createdAt || new Date().toISOString(),
        hl7Content: response.hl7Content
      };
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      throw err;
    }
  };
  
  const resendMessage = async (id: string, options = {}) => {
    try {
      return await api.resendMessage(id, options);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      throw err;
    }
  };
  
  const batchResend = async (messageIds: string[], options = {}) => {
    try {
      return await api.batchResendMessages(messageIds, options);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      throw err;
    }
  };
  
  const updateMessageContent = async (id: string, content: string) => {
    try {
      return await api.updateMessageContent(id, content);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      throw err;
    }
  };
  
  return (
    <MessageContext.Provider value={{
      messages,
      loading,
      error,
      fetchMessages,
      getMessage,
      resendMessage,
      batchResend,
      updateMessageContent
    }}>
      {children}
    </MessageContext.Provider>
  );
};

export const useMessages = (): MessageContextType => {
  const context = useContext(MessageContext);
  if (!context) {
    throw new Error('useMessages must be used within a MessageProvider');
  }
  return context;
};