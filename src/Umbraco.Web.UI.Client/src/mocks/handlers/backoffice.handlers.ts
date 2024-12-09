const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

import logoUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo.svg';
import loginLogoUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_light.svg';
import loginLogoAlternativeUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_dark.svg';
import loginBackgroundUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/login.jpg';

export const handlers = [
	rest.get(umbracoPath('/security/back-office/graphics/logo'), async (req, res, ctx) => {
		const imageBuffer = await fetch(logoUrl)
			.then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer)
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/login-logo'), async (req, res, ctx) => {
		const imageBuffer = await fetch(loginLogoUrl)
			.then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer)
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/login-logo-alternative'), async (req, res, ctx) => {
		const imageBuffer = await fetch(loginLogoAlternativeUrl)
			.then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer)
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/login-background'), async (req, res, ctx) => {
		const imageBuffer = await fetch(loginBackgroundUrl)
			.then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/jpeg'),
			ctx.body(imageBuffer)
		);
	}),
];
