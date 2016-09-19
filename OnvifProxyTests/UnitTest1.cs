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
                doc.LoadXml("<wrongTag/>");
            }
            catch (XmlException)
            {
                throw;
            }
            task = doc.DocumentElement;

            // act 
            service = new Service1();
            responce = service.CreateTask(task, beginTime, endTime, feedback);

            // assert 
            Assert.AreEqual("", responce, false); 
        }
        [TestMethod]
        public void CreateTaskTest_wrongTimeBegin_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            endTime = System.DateTime.Now;
            beginTime = System.DateTime.MaxValue;
            feedback = new TaskManager.FeedbackType();
            string responce;

            try
            {
                doc.LoadXml("<zero/>");
            }
            catch (XmlException)
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
        [TestMethod]
        public void CreateTaskTest_wrongTimeEnd_shouldFail()
        {
            // arrange 
            System.DateTime beginTime, endTime;
            TaskManager.FeedbackType feedback;
            System.Xml.XmlElement task;
            XmlDocument doc = new XmlDocument();

            endTime = System.DateTime.MinValue;
            beginTime = System.DateTime.Now;
            feedback = new TaskManager.FeedbackType();
            string responce;

            try
            {
                doc.LoadXml("<zero/>");
            }
            catch (XmlException)
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
                doc.LoadXml("<zero/>");
            }
            catch (XmlException)
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
        [TestMethod]
        public void CreateTaskTest_shouldPass()
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
                doc.LoadXml("<zero/>");
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
            // act 
            // assert 
        }
        [TestMethod]
        public void GetTaskStatus_shouldPass()
        {
            // arrange 
            // act 
            // assert 
        }
        #endregion

        #region public TaskManager.GetTaskResultsResponse GetTaskResults(TaskManager.GetTaskResultsRequest request)
        [TestMethod]
        public void GetTaskResults_wrongRequest_shouldFail()
        {
            // arrange 
            // act 
            // assert 
        }
        [TestMethod]
        public void GetTaskResults_shouldPass()
        {
            // arrange 
            // act 
            // assert 
        }
        #endregion
        
        #region public void DeleteTask(string TaskToken)
        [TestMethod]
        public void DeleteTask_shouldPass()
        {
            // arrange 
           
            // act 
           
            //assert
           
        }
        [TestMethod]
        public void DeleteTask_wrongTaskToken_shouldFail()
        {
            // arrange 
            Service1 service;
            string wrongTokenToDelete = "some_token";
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
