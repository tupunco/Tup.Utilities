using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Extensions.Internal.Tests
{
    /// <summary>
    /// FROM: https://github.com/dotnet/aspnetcore/blob/v3.1.11/src/Shared/test/Shared.Tests/ObjectMethodExecutorTest.cs
    /// </summary>
    [TestClass]
    public class ObjectMethodExecutorTests
    {
        [TestMethod]
        public void ExecuteValueMethod()
        {
            var executor = GetExecutorForMethod("ValueMethod");
            var result = executor.Execute(
                _targetObject,
                new object[] { 10, 20 });
            Assert.IsFalse(executor.IsMethodAsync);
            Assert.AreEqual(30, (int)result);
        }

        [TestMethod]
        public void ExecuteVoidValueMethod()
        {
            var executor = GetExecutorForMethod("VoidValueMethod");
            var result = executor.Execute(
                _targetObject,
                new object[] { 10 });
            Assert.IsFalse(executor.IsMethodAsync);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ExecuteValueMethodWithReturnType()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnType");
            var result = executor.Execute(
                _targetObject,
                new object[] { 10 });
            var resultObject = Assert2.IsType<TestObject>(result);
            Assert.IsFalse(executor.IsMethodAsync);
            Assert.AreEqual("Hello", resultObject.value);
        }

        [TestMethod]
        public void ExecuteValueMethodUpdateValue()
        {
            var executor = GetExecutorForMethod("ValueMethodUpdateValue");
            var parameter = new TestObject();
            var result = executor.Execute(
                _targetObject,
                new object[] { parameter });
            var resultObject = Assert2.IsType<TestObject>(result);
            Assert.IsFalse(executor.IsMethodAsync);
            Assert.AreEqual("HelloWorld", resultObject.value);
        }

        [TestMethod]
        public void ExecuteValueMethodWithReturnTypeThrowsException()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnTypeThrowsException");
            var parameter = new TestObject();
            Assert.IsFalse(executor.IsMethodAsync);
            Assert.ThrowsException<NotImplementedException>(
                        () => executor.Execute(
                            _targetObject,
                            new object[] { parameter }));
        }

        [TestMethod]
        public async Task ExecuteValueMethodAsync()
        {
            var executor = GetExecutorForMethod("ValueMethodAsync");
            var result = await executor.ExecuteAsync(
                _targetObject,
                new object[] { 10, 20 });
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreEqual(30, (int)result);
        }

        [TestMethod]
        public async Task ExecuteValueMethodWithReturnTypeAsync()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnTypeAsync");
            var result = await executor.ExecuteAsync(
                _targetObject,
                new object[] { 10 });
            var resultObject = Assert2.IsType<TestObject>(result);
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreEqual("Hello", resultObject.value);
        }

        [TestMethod]
        public async Task ExecuteValueMethodUpdateValueAsync()
        {
            var executor = GetExecutorForMethod("ValueMethodUpdateValueAsync");
            var parameter = new TestObject();
            var result = await executor.ExecuteAsync(
                _targetObject,
                new object[] { parameter });
            var resultObject = Assert2.IsType<TestObject>(result);
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreEqual("HelloWorld", resultObject.value);
        }

        [TestMethod]
        public async Task ExecuteValueMethodWithReturnTypeThrowsExceptionAsync()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnTypeThrowsExceptionAsync");
            var parameter = new TestObject();
            Assert.IsTrue(executor.IsMethodAsync);
            await Assert.ThrowsExceptionAsync<NotImplementedException>(
                    async () => await executor.ExecuteAsync(
                            _targetObject,
                            new object[] { parameter }));
        }

        [TestMethod]
        public async Task ExecuteValueMethodWithReturnVoidThrowsExceptionAsync()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnVoidThrowsExceptionAsync");
            var parameter = new TestObject();
            Assert.IsTrue(executor.IsMethodAsync);
            await Assert.ThrowsExceptionAsync<NotImplementedException>(
                    async () => await executor.ExecuteAsync(
                            _targetObject,
                            new object[] { parameter }));
        }

        [TestMethod]
        public void GetDefaultValueForParameters_ReturnsSuppliedValues()
        {
            var suppliedDefaultValues = new object[] { 123, "test value" };
            var executor = GetExecutorForMethod("MethodWithMultipleParameters", suppliedDefaultValues);
            Assert.AreEqual(suppliedDefaultValues[0], executor.GetDefaultValueForParameter(0));
            Assert.AreEqual(suppliedDefaultValues[1], executor.GetDefaultValueForParameter(1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => executor.GetDefaultValueForParameter(2));
        }

        [TestMethod]
        public void GetDefaultValueForParameters_ThrowsIfNoneWereSupplied()
        {
            var executor = GetExecutorForMethod("MethodWithMultipleParameters");
            Assert.ThrowsException<InvalidOperationException>(() => executor.GetDefaultValueForParameter(0));
        }

        [TestMethod]
        public async Task TargetMethodReturningCustomAwaitableOfReferenceType_CanInvokeViaExecute()
        {
            // Arrange
            var executor = GetExecutorForMethod("CustomAwaitableOfReferenceTypeAsync");

            // Act
            var result = await (TestAwaitable<TestObject>)executor.Execute(_targetObject, new object[] { "Hello", 123 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(TestObject), executor.AsyncResultType);
            Assert.IsNotNull(result);
            Assert.AreEqual("Hello 123", result.value);
        }

        [TestMethod]
        public async Task TargetMethodReturningCustomAwaitableOfValueType_CanInvokeViaExecute()
        {
            // Arrange
            var executor = GetExecutorForMethod("CustomAwaitableOfValueTypeAsync");

            // Act
            var result = await (TestAwaitable<int>)executor.Execute(_targetObject, new object[] { 123, 456 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(int), executor.AsyncResultType);
            Assert.AreEqual(579, result);
        }

        [TestMethod]
        public async Task TargetMethodReturningCustomAwaitableOfReferenceType_CanInvokeViaExecuteAsync()
        {
            // Arrange
            var executor = GetExecutorForMethod("CustomAwaitableOfReferenceTypeAsync");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[] { "Hello", 123 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(TestObject), executor.AsyncResultType);
            Assert.IsNotNull(result);
            Assert2.IsType<TestObject>(result);
            Assert.AreEqual("Hello 123", ((TestObject)result).value);
        }

        [TestMethod]
        public async Task TargetMethodReturningCustomAwaitableOfValueType_CanInvokeViaExecuteAsync()
        {
            // Arrange
            var executor = GetExecutorForMethod("CustomAwaitableOfValueTypeAsync");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[] { 123, 456 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(int), executor.AsyncResultType);
            Assert.IsNotNull(result);
            Assert2.IsType<int>(result);
            Assert.AreEqual(579, (int)result);
        }

        [TestMethod]
        public async Task TargetMethodReturningAwaitableOfVoidType_CanInvokeViaExecuteAsync()
        {
            // Arrange
            var executor = GetExecutorForMethod("VoidValueMethodAsync");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[] { 123 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(void), executor.AsyncResultType);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TargetMethodReturningAwaitableWithICriticalNotifyCompletion_UsesUnsafeOnCompleted()
        {
            // Arrange
            var executor = GetExecutorForMethod("CustomAwaitableWithICriticalNotifyCompletion");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[0]);

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(string), executor.AsyncResultType);
            Assert.AreEqual("Used UnsafeOnCompleted", (string)result);
        }

        [TestMethod]
        public async Task TargetMethodReturningAwaitableWithoutICriticalNotifyCompletion_UsesOnCompleted()
        {
            // Arrange
            var executor = GetExecutorForMethod("CustomAwaitableWithoutICriticalNotifyCompletion");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[0]);

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(string), executor.AsyncResultType);
            Assert.AreEqual("Used OnCompleted", (string)result);
        }

        [TestMethod]
        public async Task TargetMethodReturningValueTaskOfValueType_CanBeInvokedViaExecute()
        {
            // Arrange
            var executor = GetExecutorForMethod("ValueTaskOfValueType");

            // Act
            var result = await (ValueTask<int>)executor.Execute(_targetObject, new object[] { 123 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(int), executor.AsyncResultType);
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public async Task TargetMethodReturningValueTaskOfReferenceType_CanBeInvokedViaExecute()
        {
            // Arrange
            var executor = GetExecutorForMethod("ValueTaskOfReferenceType");

            // Act
            var result = await (ValueTask<string>)executor.Execute(_targetObject, new object[] { "test result" });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(string), executor.AsyncResultType);
            Assert.AreEqual("test result", result);
        }

        [TestMethod]
        public async Task TargetMethodReturningValueTaskOfValueType_CanBeInvokedViaExecuteAsync()
        {
            // Arrange
            var executor = GetExecutorForMethod("ValueTaskOfValueType");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[] { 123 });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(int), executor.AsyncResultType);
            Assert.IsNotNull(result);
            Assert.AreEqual(123, (int)result);
        }

        [TestMethod]
        public async Task TargetMethodReturningValueTaskOfReferenceType_CanBeInvokedViaExecuteAsync()
        {
            // Arrange
            var executor = GetExecutorForMethod("ValueTaskOfReferenceType");

            // Act
            var result = await executor.ExecuteAsync(_targetObject, new object[] { "test result" });

            // Assert
            Assert.IsTrue(executor.IsMethodAsync);
            Assert.AreSame(typeof(string), executor.AsyncResultType);
            Assert.AreEqual("test result", result);
        }

        //[TestMethod]
        //public async Task TargetMethodReturningFSharpAsync_CanBeInvokedViaExecute()
        //{
        //    // Arrange
        //    var executor = GetExecutorForMethod("FSharpAsyncMethod");

        //    // Act
        //    var fsharpAsync = (FSharpAsync<string>)executor.Execute(_targetObject, new object[] { "test result" });
        //    var result = await FSharpAsync.StartAsTask(fsharpAsync,
        //        FSharpOption<TaskCreationOptions>.None,
        //        FSharpOption<CancellationToken>.None);

        //    // Assert
        //    Assert.IsTrue(executor.IsMethodAsync);
        //    Assert.AreSame(typeof(string), executor.AsyncResultType);
        //    Assert.AreEqual("test result", result);
        //}

        //[TestMethod]
        //public async Task TargetMethodReturningFailingFSharpAsync_CanBeInvokedViaExecute()
        //{
        //    // Arrange
        //    var executor = GetExecutorForMethod("FSharpAsyncFailureMethod");

        //    // Act
        //    var fsharpAsync = (FSharpAsync<string>)executor.Execute(_targetObject, new object[] { "test result" });
        //    var resultTask = FSharpAsync.StartAsTask(fsharpAsync,
        //        FSharpOption<TaskCreationOptions>.None,
        //        FSharpOption<CancellationToken>.None);

        //    // Assert
        //    Assert.IsTrue(executor.IsMethodAsync);
        //    Assert.AreSame(typeof(string), executor.AsyncResultType);

        //    var exception = await Assert.ThrowsExceptionAsync<AggregateException>(async () => await resultTask);
        //    Assert2.IsType<InvalidOperationException>(exception.InnerException);
        //    Assert.AreEqual("Test exception", exception.InnerException.Message);
        //}

        //[TestMethod]
        //public async Task TargetMethodReturningFSharpAsync_CanBeInvokedViaExecuteAsync()
        //{
        //    // Arrange
        //    var executor = GetExecutorForMethod("FSharpAsyncMethod");

        //    // Act
        //    var result = await executor.ExecuteAsync(_targetObject, new object[] { "test result" });

        //    // Assert
        //    Assert.IsTrue(executor.IsMethodAsync);
        //    Assert.AreSame(typeof(string), executor.AsyncResultType);
        //    Assert.AreEqual("test result", result);
        //}

        //[TestMethod]
        //public async Task TargetMethodReturningFailingFSharpAsync_CanBeInvokedViaExecuteAsync()
        //{
        //    // Arrange
        //    var executor = GetExecutorForMethod("FSharpAsyncFailureMethod");

        //    // Act
        //    var resultTask = executor.ExecuteAsync(_targetObject, new object[] { "test result" });

        //    // Assert
        //    Assert.IsTrue(executor.IsMethodAsync);
        //    Assert.AreSame(typeof(string), executor.AsyncResultType);

        //    var exception = await Assert.ThrowsExceptionAsync<AggregateException>(async () => await resultTask);
        //    Assert2.IsType<InvalidOperationException>(exception.InnerException);
        //    Assert.AreEqual("Test exception", exception.InnerException.Message);
        //}

        #region 基础数据

        private TestObject _targetObject = new TestObject();
        private TypeInfo targetTypeInfo = typeof(TestObject).GetTypeInfo();

        private ObjectMethodExecutor GetExecutorForMethod(string methodName)
        {
            var method = typeof(TestObject).GetMethod(methodName);
            return ObjectMethodExecutor.Create(method, targetTypeInfo);
        }

        private ObjectMethodExecutor GetExecutorForMethod(string methodName, object[] parameterDefaultValues)
        {
            var method = typeof(TestObject).GetMethod(methodName);
            return ObjectMethodExecutor.Create(method, targetTypeInfo, parameterDefaultValues);
        }

        public class TestObject
        {
            public string value;

            public int ValueMethod(int i, int j)
            {
                return i + j;
            }

            public void VoidValueMethod(int i)
            {
            }

            public TestObject ValueMethodWithReturnType(int i)
            {
                return new TestObject() { value = "Hello" }; ;
            }

            public TestObject ValueMethodWithReturnTypeThrowsException(TestObject i)
            {
                throw new NotImplementedException("Not Implemented Exception");
            }

            public TestObject ValueMethodUpdateValue(TestObject parameter)
            {
                parameter.value = "HelloWorld";
                return parameter;
            }

            public Task<int> ValueMethodAsync(int i, int j)
            {
                return Task.FromResult<int>(i + j);
            }

            public async Task VoidValueMethodAsync(int i)
            {
                await ValueMethodAsync(3, 4);
            }

            public Task<TestObject> ValueMethodWithReturnTypeAsync(int i)
            {
                return Task.FromResult<TestObject>(new TestObject() { value = "Hello" });
            }

            public async Task ValueMethodWithReturnVoidThrowsExceptionAsync(TestObject i)
            {
                await Task.CompletedTask;
                throw new NotImplementedException("Not Implemented Exception");
            }

            public async Task<TestObject> ValueMethodWithReturnTypeThrowsExceptionAsync(TestObject i)
            {
                await Task.CompletedTask;
                throw new NotImplementedException("Not Implemented Exception");
            }

            public Task<TestObject> ValueMethodUpdateValueAsync(TestObject parameter)
            {
                parameter.value = "HelloWorld";
                return Task.FromResult<TestObject>(parameter);
            }

            public TestAwaitable<TestObject> CustomAwaitableOfReferenceTypeAsync(
                string input1,
                int input2)
            {
                return new TestAwaitable<TestObject>(new TestObject
                {
                    value = $"{input1} {input2}"
                });
            }

            public TestAwaitable<int> CustomAwaitableOfValueTypeAsync(
                int input1,
                int input2)
            {
                return new TestAwaitable<int>(input1 + input2);
            }

            public TestAwaitableWithICriticalNotifyCompletion CustomAwaitableWithICriticalNotifyCompletion()
            {
                return new TestAwaitableWithICriticalNotifyCompletion();
            }

            public TestAwaitableWithoutICriticalNotifyCompletion CustomAwaitableWithoutICriticalNotifyCompletion()
            {
                return new TestAwaitableWithoutICriticalNotifyCompletion();
            }

            public ValueTask<int> ValueTaskOfValueType(int result)
            {
                return new ValueTask<int>(result);
            }

            public ValueTask<string> ValueTaskOfReferenceType(string result)
            {
                return new ValueTask<string>(result);
            }

            public void MethodWithMultipleParameters(int valueTypeParam, string referenceTypeParam)
            {
            }

            //public FSharpAsync<string> FSharpAsyncMethod(string parameter)
            //{
            //    return FSharpAsync.AwaitTask(Task.FromResult(parameter));
            //}

            //public FSharpAsync<string> FSharpAsyncFailureMethod(string parameter)
            //{
            //    return FSharpAsync.AwaitTask(
            //        Task.FromException<string>(new InvalidOperationException("Test exception")));
            //}
        }

        public class TestAwaitable<T>
        {
            private T _result;
            private bool _isCompleted;
            private List<Action> _onCompletedCallbacks = new List<Action>();

            public TestAwaitable(T result)
            {
                _result = result;

                // Simulate a brief delay before completion
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(100);
                    SetCompleted();
                });
            }

            private void SetCompleted()
            {
                _isCompleted = true;

                foreach (var callback in _onCompletedCallbacks)
                {
                    callback();
                }
            }

            public TestAwaiter GetAwaiter()
            {
                return new TestAwaiter(this);
            }

            public struct TestAwaiter : INotifyCompletion
            {
                private TestAwaitable<T> _owner;

                public TestAwaiter(TestAwaitable<T> owner) : this()
                {
                    _owner = owner;
                }

                public bool IsCompleted => _owner._isCompleted;

                public void OnCompleted(Action continuation)
                {
                    if (_owner._isCompleted)
                    {
                        continuation();
                    }
                    else
                    {
                        _owner._onCompletedCallbacks.Add(continuation);
                    }
                }

                public T GetResult()
                {
                    return _owner._result;
                }
            }
        }

        public class TestAwaitableWithICriticalNotifyCompletion
        {
            public TestAwaiterWithICriticalNotifyCompletion GetAwaiter()
                => new TestAwaiterWithICriticalNotifyCompletion();
        }

        public class TestAwaitableWithoutICriticalNotifyCompletion
        {
            public TestAwaiterWithoutICriticalNotifyCompletion GetAwaiter()
                => new TestAwaiterWithoutICriticalNotifyCompletion();
        }

        public class TestAwaiterWithICriticalNotifyCompletion
            : CompletionTrackingAwaiterBase, ICriticalNotifyCompletion
        {
        }

        public class TestAwaiterWithoutICriticalNotifyCompletion
            : CompletionTrackingAwaiterBase, INotifyCompletion
        {
        }

        public class CompletionTrackingAwaiterBase
        {
            private string _result;

            public bool IsCompleted { get; private set; }

            public string GetResult() => _result;

            public void OnCompleted(Action continuation)
            {
                _result = "Used OnCompleted";
                IsCompleted = true;
                continuation();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _result = "Used UnsafeOnCompleted";
                IsCompleted = true;
                continuation();
            }
        }

        #endregion
    }
}