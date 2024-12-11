const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const logoUrl = './src/mocks/handlers/backoffice/assets/logo.svg';
const loginLogoUrl = './src/mocks/handlers/backoffice/assets/logo_light.svg';
const loginLogoAlternativeUrl = './src/mocks/handlers/backoffice/assets/logo_dark.svg';
const loginBackgroundUrl = './src/mocks/handlers/backoffice/assets/login.jpg';

export const handlers = [
	rest.get(umbracoPath('/security/back-office/graphics/logo'), async (req, res, ctx) => {
		const imageBuffer = await fetch(logoUrl).then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer),
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/login-logo'), async (req, res, ctx) => {
		const imageBuffer = await fetch(loginLogoUrl).then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer),
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/login-logo-alternative'), async (req, res, ctx) => {
		const imageBuffer = await fetch(loginLogoAlternativeUrl).then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/svg+xml'),
			ctx.body(imageBuffer),
		);
	}),
	rest.get(umbracoPath('/security/back-office/graphics/login-background'), async (req, res, ctx) => {
		const imageBuffer = await fetch(loginBackgroundUrl).then((res) => res.arrayBuffer());

		return res(
			ctx.set('Content-Length', imageBuffer.byteLength.toString()),
			ctx.set('Content-Type', 'image/jpeg'),
			ctx.body(imageBuffer),
		);
	}),
];
