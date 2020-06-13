USE [tcow]
GO

/****** Object:  Table [dbo].[Inventory]    Script Date: 2/13/2020 2:26:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Inventory](
	[InventoryId] [int] IDENTITY(1,1) NOT NULL,
	[CollectorNumber] [nvarchar](50) NOT NULL,
	[TCGProdId] [int] NULL,
	[CardName] [nvarchar](150) NOT NULL,
	[SetCode] [nvarchar](10) NOT NULL,
	[SetName] [nvarchar](50) NOT NULL,
	[NMQty] [int] NOT NULL,
	[SPQty] [int] NOT NULL,
	[PLDQty] [int] NOT NULL,
	[HPQty] [int] NOT NULL,
	[NMFoilQty] [int] NOT NULL,
	[SPFoilQty] [int] NOT NULL,
	[PLDFoilQty] [int] NOT NULL,
	[HPFoilQty] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InventoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Customer]    Script Date: 2/13/2020 2:56:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customer](
	[CustId] [int] IDENTITY(0,1) NOT NULL,
	[CustName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CustId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



