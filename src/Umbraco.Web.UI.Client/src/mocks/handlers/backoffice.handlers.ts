const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

import logoUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_light.svg';
import logoAlternativeUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_dark.svg';
import backgroundUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/login.jpg';

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
	rest.get(umbracoPath('/security/back-office/graphics/logo-alternative'), async (req, res, ctx) => {
		const imageBuffer = await fetch(logoAlternativeUrl)
			.then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer)
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/background'), async (req, res, ctx) => {
		const imageBuffer = await fetch(backgroundUrl)
			.then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/jpeg'),
			ctx.body(imageBuffer)
		);
	}),
];
