import { rest } from 'msw';
import { searchResultMockData, getIndexByName, PagedIndexers } from '../data/examine.data';

import { getGroupByName, healthGroups, healthGroupsWithoutResult } from '../data/health-check.data';

import { umbracoPath } from '@umbraco-cms/utils';
import { HealthCheckGroup, PagedHealthCheckGroup, StatusResultType } from '@umbraco-cms/backend-api';

export const handlers = [
	rest.get(umbracoPath('/health-check-group'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedHealthCheckGroup>({ total: 9999, items: healthGroupsWithoutResult })
		);
	}),

	rest.get(umbracoPath('/health-check-group/:name'), (_req, res, ctx) => {
		const name = _req.params.name as string;

		if (!name) return;
		const group = getGroupByName(name);

		if (group) {
			return res(ctx.status(200), ctx.json<HealthCheckGroup>(group));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.post(umbracoPath('/postHealthCheckExecuteAction'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds
		return res(
			// Respond with a 200 status code
			ctx.status(200)
		);
	}),

	rest.get(umbracoPath('/health-check/security'), (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json<any>([
				{
					alias: 'applicationUrlConfiguration',
					results: [
						{
							message: `The appSetting 'Umbraco:CMS:WebRouting:UmbracoApplicationUrl' is not set`,
							resultType: StatusResultType.WARNING,
							readMoreLink: 'https://umbra.co/healthchecks-umbraco-application-url',
						},
					],
				},
				{
					alias: 'clickJackingProtection',
					results: [
						{
							message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
							resultType: StatusResultType.ERROR,
							readMoreLink: 'https://umbra.co/healthchecks-click-jacking',
						},
					],
				},
				{
					alias: 'contentSniffingProtection',
					results: [
						{
							message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
							resultType: StatusResultType.ERROR,
							readMoreLink: 'https://umbra.co/healthchecks-no-sniff',
						},
					],
				},
				{
					alias: 'cookieHijackingProtection',
					results: [
						{
							message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
							resultType: StatusResultType.ERROR,
							readMoreLink: 'https://umbra.co/healthchecks-hsts',
						},
					],
				},
				{
					alias: 'crossSiteProtection',
					results: [
						{
							message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
							resultType: StatusResultType.ERROR,
							readMoreLink: 'https://umbra.co/healthchecks-xss-protection',
						},
					],
				},
				{
					alias: 'excessiveHeaders',
					results: [
						{
							message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
							resultType: StatusResultType.WARNING,
							readMoreLink: 'https://umbra.co/healthchecks-excessive-headers',
						},
					],
				},
				{
					alias: 'HttpsConfiguration',
					results: [
						{
							message: `You are currently viewing the site using HTTPS scheme`,
							resultType: StatusResultType.SUCCESS,
						},
						{
							message: `The appSetting 'Umbraco:CMS:Global:UseHttps' is set to 'False' in your appSettings.json file,
						your cookies are not marked as secure.`,
							resultType: StatusResultType.ERROR,
							readMoreLink: 'https://umbra.co/healthchecks-https-config',
						},
						{
							message: `Error pinging the URL https://localhost:44361/ - 'The SSL connection could not be established,
						see inner exception.'"`,
							resultType: StatusResultType.ERROR,
							readMoreLink: 'https://umbra.co/healthchecks-https-request',
						},
					],
				},
			])
		);
	}),
];
