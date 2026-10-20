import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.examineManagement.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.examineManagement.clickExamineManagementTab();
});

test('can view indexers information', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allIndexersData = await umbracoApi.indexer.getAll();
  const indexerCount = allIndexersData.total;

  // Assert
  for (const index of allIndexersData.items) {
    await umbracoUi.examineManagement.doesIndexersHaveText(index.name);
  }
  await umbracoUi.examineManagement.doesIndexersHaveCount(indexerCount);
});

test('can view the details of an index', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allIndexersData = await umbracoApi.indexer.getAll();
  for (const index of allIndexersData.items) {
    const indexName = index.name;
    const indexData = await umbracoApi.indexer.getByIndexName(indexName);

    // Act
    await umbracoUi.examineManagement.clickIndexByName(indexName);

    // Assert
    await umbracoUi.examineManagement.doesIndexHaveHealthStatus(indexName, indexData.healthStatus.status);
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('documentCount', indexData.documentCount.toString());
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('fieldCount', indexData.fieldCount.toString());
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('CommitCount', indexData.providerProperties.CommitCount.toString());
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('DefaultAnalyzer', indexData.providerProperties.DefaultAnalyzer);
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('LuceneDirectory', indexData.providerProperties.LuceneDirectory);
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('DirectoryFactory', indexData.providerProperties.DirectoryFactory);
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('EnableDefaultEventHandler', indexData.providerProperties.EnableDefaultEventHandler.toString());
    await umbracoUi.examineManagement.doesIndexPropertyHaveValue('PublishedValuesOnly', indexData.providerProperties.PublishedValuesOnly.toString());
    await umbracoUi.goBackPage();
  }
});
