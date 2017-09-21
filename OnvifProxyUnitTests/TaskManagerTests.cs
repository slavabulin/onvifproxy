// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.XmlConfiguration;
using System.Xml;
using OnvifProxy;
using System.ServiceModel;


namespace OnvifProxyTests
{
    [TestClass]
    public class TaskManagerTests
    {
        #region public string CreateTask(System.Xml.XmlElement Task, System.DateTime TimeBegin, System.DateTime TimeEnd, TaskManager.FeedbackType Feedback)
        
        [TestMethod]
        public void CreateTaskTest_wrongTask_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            beginTime = System.DateTime.MinValue;
            endTime = System.DateTime.MaxValue;
            feedback = new TaskManager.FeedbackType();
            string responce;
            Service1 service;

            try
            {
                doc.LoadXml("<tm:T xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:M>camera 1</tm:M>"
                    + "</tm:T>");
            }
            catch (XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            service = new Service1();
            try
            {
                responce = service.CreateTask(task, beginTime, endTime, feedback);
            }
            catch(FaultException fe)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CreateTaskTest_wrongTimeBegin_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            endTime = System.DateTime.MaxValue;
            beginTime = System.DateTime.MinValue;
            feedback = new TaskManager.FeedbackType();
            string responce;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>");
            }
            catch (XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            Service1 service = new Service1();
            try
            {
                responce = service.CreateTask(task, beginTime, endTime, feedback);
            }
            catch (FaultException fe)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CreateTaskTest_TimeBeginTimeEndChanged_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            endTime = System.DateTime.Now;
            beginTime = System.DateTime.MaxValue;
            feedback = new TaskManager.FeedbackType();
            string responce = String.Empty;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>");
            }
            catch (XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            Service1 service = new Service1();
            try
            {
                responce = service.CreateTask(task, beginTime, endTime, feedback);
            }
            catch(FaultException fe)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CreateTaskTest_wrongTimeEnd_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            endTime = System.DateTime.MinValue.AddHours(1);
            beginTime = System.DateTime.MinValue;
            feedback = new TaskManager.FeedbackType();
            string responce;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>");
            }
            catch (XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            Service1 service = new Service1();
            try
            {
                responce = service.CreateTask(task, beginTime, endTime, feedback);
            }
            catch (FaultException fe)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CreateTaskTest_wrongFeedback_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            beginTime = System.DateTime.MinValue;
            endTime = System.DateTime.MaxValue;
            //feedback = new TaskManager.FeedbackType();
            feedback = null;
            string responce;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>");
            }
            catch (XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            Service1 service = new Service1();
            try
            {
                responce = service.CreateTask(task, beginTime, endTime, feedback);
            }
            catch(FaultException fe)
            {
                return;
            }           

            // assert 
            Assert.Fail();
        }

        [TestMethod]
        public void CreateTaskTest_shouldPass()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            beginTime = System.DateTime.Now.AddHours(1);
            endTime = System.DateTime.MaxValue;
            feedback = new TaskManager.FeedbackType();
            string responce;

            try
            {
                doc.LoadXml("<tm:Task xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    +"<tm:ScheduledRecordingTask>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>"
                    + "</tm:Task>");
            }
            catch(XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            Service1 service = new Service1();
            responce = service.CreateTask(task, beginTime, endTime, feedback);

            // assert 
            Assert.AreEqual("", responce, false);
        }
        #endregion

        #region public TaskManager.GetTaskStatusResponse GetTaskStatus(TaskManager.GetTaskStatusRequest request)

        [TestMethod]
        public void GetTaskStatus_wrongRequest_shouldFail()
        {
            // arrange 
            Service1 service = new Service1();
            var badTaskStatusRequest = new TaskManager.GetTaskStatusRequest();
            badTaskStatusRequest.TaskToken = String.Empty;
            // act 
            try
            {
                //service.GetTaskStatus(null);
                service.GetTaskStatus(badTaskStatusRequest);
            }
            catch(FaultException fe)
            {
                return;
            }

            // assert 
            Assert.Fail();
            
        }

        [TestMethod]
        public void GetTaskStatus_shouldPass()
        {
            // arrange 
            Service1 service = new Service1();
            string createTaskResponse;
            System.DateTime beginTime, endTime;
            XmlElement task;
            TaskManager.FeedbackType feedback;
            XmlDocument doc = new XmlDocument();
            var getTaskStatusResponse = new TaskManager.GetTaskStatusResponse();

            beginTime = System.DateTime.MinValue;
            endTime = System.DateTime.Now;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>");
            }
            catch (XmlException ex)
            {
                throw;
            }
            task = doc.DocumentElement;
            feedback = new TaskManager.FeedbackType();
            // act 
            try
            {
                createTaskResponse = service.CreateTask(task, beginTime, endTime, feedback);
                getTaskStatusResponse = service.GetTaskStatus(new TaskManager.GetTaskStatusRequest(createTaskResponse));
            }
            catch (FaultException fe)
            {
                throw;
            }
            catch (Exception e)
            {
                throw;
            }
            if (getTaskStatusResponse.Status.GetType() == typeof(TaskManager.TaskStatus))
            {
                return;
            }

            // assert 
            Assert.Fail();
            
        }
        #endregion

        #region public TaskManager.GetTaskResultsResponse GetTaskResults(TaskManager.GetTaskResultsRequest request)
        [TestMethod]
        public void GetTaskResults_wrongRequest_shouldFail()
        {
            // arrange 
            // act 
            // assert 
            Assert.Fail();
        }

        [TestMethod]
        public void GetTaskResults_shouldPass()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            beginTime = System.DateTime.MinValue;
            endTime = System.DateTime.MaxValue;
            feedback = new TaskManager.FeedbackType();
            string createTaskResponse = null;
            TaskManager.GetTaskResultsResponse getTaskResultsResponse = null;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    + "<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    + "</tm:ScheduledRecordingTask>");
            }
            catch (XmlException ex)
            {
                throw;
            }
            task = doc.DocumentElement;

            Service1 service = new Service1();
            try
            {
                createTaskResponse = service.CreateTask(task, beginTime, endTime, feedback);
                getTaskResultsResponse = service.GetTaskResults(new TaskManager.GetTaskResultsRequest(createTaskResponse));
                if(getTaskResultsResponse!=null)
                {
                    return;
                }
            }
            catch (FaultException fe)
            {
                throw;
            }
            catch (Exception ee)
            {
                Assert.Fail();
            }
            finally
            {
                if (createTaskResponse != null)
                {
                    try
                    {
                        service.DeleteTask(createTaskResponse);
                    }
                    catch(Exception )
                    {
                        throw;                    
                    }                        
                }                
            }
        }
        #endregion
        
        #region public void DeleteTask(string TaskToken)

        [TestMethod]
        public void DeleteTask_shouldPass()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            beginTime = System.DateTime.MinValue;
            endTime = System.DateTime.MaxValue;
            feedback = new TaskManager.FeedbackType();
            string responce;

            try
            {
                doc.LoadXml("<tm:ScheduledRecordingTask xmlns:tm ='urn:ias:cvss:tm:1.0'>"
                    +"<tm:MediaSourceToken>camera 1</tm:MediaSourceToken>"
                    +"</tm:ScheduledRecordingTask>");
            }
            catch(XmlException ex)
            {
                throw;
            }
            task = doc.DocumentElement;

            Service1 service = new Service1();
            try
            {
                responce = service.CreateTask(task, beginTime, endTime, feedback);
                service.DeleteTask(responce);
            }
            catch(FaultException fe)
            {
                throw;
            }
            catch(Exception ee)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void DeleteTask_wrongTaskToken_shouldFail()
        {
            // arrange 
            Service1 service;
            string wrongTokenToDelete = Guid.NewGuid().ToString();
            // act 
            service = new Service1();
            try
            {
                service.DeleteTask(wrongTokenToDelete);
            }
            catch (FaultException fe)
            {
                return;
            }
            //assert
            Assert.Fail(); 
        }
        [TestMethod]
        public void DeleteTask_whitespaceTaskToken_shouldFail()
        {
            // arrange 
            Service1 service;
            string wrongTokenToDelete = String.Empty;
            // act 
            service = new Service1();
            try
            {
                service.DeleteTask(wrongTokenToDelete);
            }
            catch (FaultException fe)
            {
                return;
            }
            //assert
            Assert.Fail();
        }
        #endregion
    }
}
