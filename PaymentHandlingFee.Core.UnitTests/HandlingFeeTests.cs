using System;
using System.Threading;
using System.Threading.Tasks;
using ExpectedObjects;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.HandlingFee;
using Tgs.Payment.HandlingFee.ApplicationServices;
using Tgs.Payment.HandlingFee.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace PaymentHandlingFee.Core.UnitTests;

[TestClass]
public class HandlingFeeTests
{
	[TestMethod]
	public async Task Get_DepositForm_Info()
	{
		var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
		var sut = new GetDepositFormQueryService(fakePaymentServiceClient);

		var query = new GetDepositFormQuery(Guid.NewGuid());

		_ = await sut.GetAsync(query);

		_ = fakePaymentServiceClient.Received(1)
			.GetDepositFormAsync(Arg.Is(query.FormId), Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task Get_WithdrawForm_Info()
	{
		var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
		var sut = new GetWithdrawFormQueryService(fakePaymentServiceClient);

		var query = new GetWithdrawFormQuery(Guid.NewGuid());

		_ = await sut.GetAsync(query);

		_ = fakePaymentServiceClient.Received(1)
			.GetWithdrawFormAsync(Arg.Is(query.FormId), Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task Save_Handling_Fee()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var sut = new SaveHandlingFeeCommandService(fakePaymentRepository);

		var command = new SaveHandlingFeeCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			PaymentType.Deposit,
			1000M,
			10M,
			UserType.Player,
			"playerCode",
			DateTime.UtcNow,
			DateTime.UtcNow,
			null);

		var expected = new
		{
			command.FormId,
			command.Type,
			command.Amount,
			command.Fee,
			command.UserType,
			command.UserId,
			command.CreatedOn,
			command.ApprovedOn
		}.ToExpectedObject();

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.AddPaymentHandlingFeeRecordAsync(
				Arg.Is<PaymentHandlingFeeCreateInfo>(x => expected.Matches(x)),
				Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task Handle_Approved_Deposit_Fee()
	{
		var fakeGetDepositFormService = Substitute.For<IGetService<GetDepositFormQuery, DepositFormInfo?>>();
		var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();
		var fakeSaveHandlingFeeService = Substitute.For<ICommandService<SaveHandlingFeeCommand>>();
		var sut = new DepositStatusChangeHandler(
			fakeGetDepositFormService,
			fakeGetBrandInfoService,
			fakeSaveHandlingFeeService);

		var eventData = new FormEventEx
		{
			BrandId = Guid.NewGuid(),
			FormId = Guid.NewGuid(),
			Status = PaymentFlowState.WaitApprove.ToString()
		};

		var rowTime = DateTimeOffset.Now;

		fakeGetDepositFormService
			.GetAsync(
				Arg.Is<GetDepositFormQuery>(query => query.FormId == eventData.FormId),
				Arg.Any<CancellationToken>())
			.Returns(new DepositFormInfo
			{
				Id = eventData.FormId,
				ActualAmount = 1000M,
				Amount = 500M,
				BrandId = eventData.BrandId,
				Status = PaymentFlowState.Approved,
				PlayerCode = "tester",
				CreatedOn = rowTime,
				ReviewedOn = rowTime
			});

		fakeGetBrandInfoService
			.GetAsync(
				Arg.Is<GetBrandInfoQuery>(query => query.BrandId == eventData.BrandId),
				Arg.Any<CancellationToken>())
			.Returns(new BrandInfo(eventData.BrandId, 10M, 20M));

		var expected = new
		{
			FormId = eventData.FormId.ToGuid(),
			BrandId = eventData.BrandId.ToGuid(),
			Type = PaymentType.Deposit,
			Amount = 1000M,
			Fee = 1000M * 10M * 0.001M,
			UserType = UserType.Player,
			UserId = "tester",
			CreatedOn = rowTime.UtcDateTime,
			ApprovedOn = rowTime.UtcDateTime
		}.ToExpectedObject();

		await sut.HandleAsync(eventData.ToByteArray());

		_ = fakeSaveHandlingFeeService.Received(1)
			.ExecuteAsync(
				Arg.Is<SaveHandlingFeeCommand>(command => expected.Matches(command)),
				Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task Hande_Approved_Withdraw_Fee()
	{
		var fakeGetWithdrawFormService = Substitute.For<IGetService<GetWithdrawFormQuery, WithdrawFormInfo?>>();
		var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();
		var fakeSaveHandlingFeeService = Substitute.For<ICommandService<SaveHandlingFeeCommand>>();
		var sut = new WithdrawStatusChangeHandler(
			fakeGetWithdrawFormService,
			fakeGetBrandInfoService,
			fakeSaveHandlingFeeService);

		var eventData = new FormEventEx
		{
			BrandId = Guid.NewGuid(),
			FormId = Guid.NewGuid(),
			Status = PaymentFlowState.WaitApprove.ToString()
		};

		var rowTime = DateTimeOffset.Now;
		var userType = UserType.Player;

		fakeGetWithdrawFormService
			.GetAsync(
				Arg.Is<GetWithdrawFormQuery>(query => query.FormId == eventData.FormId),
				Arg.Any<CancellationToken>())
			.Returns(new WithdrawFormInfo
			{
				Id = eventData.FormId,
				ActualAmount = 1000M,
				Amount = 500M,
				BrandId = eventData.BrandId,
				Status = PaymentFlowState.Approved,
				UserType = userType,
				UserId = "tester",
				CreatedOn = rowTime,
				ReviewedOn = rowTime
			});

		fakeGetBrandInfoService
			.GetAsync(
				Arg.Is<GetBrandInfoQuery>(query => query.BrandId == eventData.BrandId),
				Arg.Any<CancellationToken>())
			.Returns(new BrandInfo(eventData.BrandId, 10M, 20M));

		var expected = new
		{
			FormId = eventData.FormId.ToGuid(),
			BrandId = eventData.BrandId.ToGuid(),
			Type = PaymentType.Withdraw,
			Amount = 1000M,
			Fee = 1000M * 20M * 0.001M,
			UserType = userType,
			UserId = "tester",
			CreatedOn = rowTime.UtcDateTime,
			ApprovedOn = rowTime.UtcDateTime
		}.ToExpectedObject();

		await sut.HandleAsync(eventData.ToByteArray());

		_ = fakeSaveHandlingFeeService.Received(1)
			.ExecuteAsync(
				Arg.Is<SaveHandlingFeeCommand>(command => expected.Matches(command)),
				Arg.Any<CancellationToken>());
	}
}
