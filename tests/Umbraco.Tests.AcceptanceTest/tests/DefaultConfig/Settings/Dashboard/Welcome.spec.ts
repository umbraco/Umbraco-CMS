import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.welcomeDashboard.goToSection(ConstantHelper.sections.settings);
});

test('can click on buttons', async ({umbracoUi}) => {
  // Arrange
  const getTheHelpYouNeedDocumentationUrl = 'https://docs.umbraco.com/umbraco-cms';
  const goToTheForumUrl = 'https://our.umbraco.com/forum/';
  const chatWithTheCommunityUrl = 'https://discord.umbraco.com';
  const getCertifiedUrl = 'https://umbraco.com/training/';
  const getTheHelpYouNeedSupportUrl = 'https://umbraco.com/support/';
  const watchTheVideosUrl = 'https://www.youtube.com/c/UmbracoLearningBase';

  // Act
  await umbracoUi.welcomeDashboard.clickWelcomeTab();

  // Assert
  await umbracoUi.welcomeDashboard.doesButtonWithLabelInBoxHaveLink('Get the help you need', 'Documentation', getTheHelpYouNeedDocumentationUrl);
  await umbracoUi.welcomeDashboard.doesButtonWithLabelInBoxHaveLink('Go to the forum', 'Community', goToTheForumUrl);
  await umbracoUi.welcomeDashboard.doesButtonWithLabelInBoxHaveLink('Chat with the community', 'Community', chatWithTheCommunityUrl);
  await umbracoUi.welcomeDashboard.doesButtonWithLabelInBoxHaveLink('Get Certified', 'Training', getCertifiedUrl);
  await umbracoUi.welcomeDashboard.doesButtonWithLabelInBoxHaveLink('Get the help you need', 'Support', getTheHelpYouNeedSupportUrl);
  await umbracoUi.welcomeDashboard.doesButtonWithLabelInBoxHaveLink('Watch the videos', 'Videos', watchTheVideosUrl);
});
