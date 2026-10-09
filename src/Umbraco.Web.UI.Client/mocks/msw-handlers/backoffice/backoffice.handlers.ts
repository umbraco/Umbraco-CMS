const { http, HttpResponse } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const logoUrl = './umbraco/backoffice/assets/logo.svg';
const logoAlternativeUrl = './umbraco/backoffice/assets/logo_blue.svg';
const loginLogoUrl = './umbraco/backoffice/assets/logo_light.svg';
const loginLogoAlternativeUrl = './umbraco/backoffice/assets/logo_dark.svg';
const loginBackgroundUrl = './umbraco/backoffice/assets/login.jpg';

export const handlers = [
	http.get(umbracoPath('/security/back-office/graphics/logo'), async () => {
		const imageBuffer = await fetch(logoUrl).then((res) => res.arrayBuffer());

		return new HttpResponse(imageBuffer, {
			headers: {
				'Content-Length': imageBuffer.byteLength.toString(),
				'Content-Type': 'image/svg+xml',
			},
		});
	}),
	http.get(umbracoPath('/security/back-office/graphics/logo-alternative'), async () => {
		const imageBuffer = await fetch(logoAlternativeUrl).then((res) => res.arrayBuffer());

		return new HttpResponse(imageBuffer, {
			headers: {
				'Content-Length': imageBuffer.byteLength.toString(),
				'Content-Type': 'image/svg+xml',
			},
		});
	}),
	http.get(umbracoPath('/security/back-office/graphics/login-logo'), async () => {
		const imageBuffer = await fetch(loginLogoUrl).then((res) => res.arrayBuffer());

		return new HttpResponse(imageBuffer, {
			headers: {
				'Content-Length': imageBuffer.byteLength.toString(),
				'Content-Type': 'image/svg+xml',
			},
		});
	}),
	http.get(umbracoPath('/security/back-office/graphics/login-logo-alternative'), async () => {
		const imageBuffer = await fetch(loginLogoAlternativeUrl).then((res) => res.arrayBuffer());

		return new HttpResponse(imageBuffer, {
			headers: {
				'Content-Length': imageBuffer.byteLength.toString(),
				'Content-Type': 'image/svg+xml',
			},
		});
	}),
	http.get(umbracoPath('/security/back-office/graphics/login-background'), async () => {
		const imageBuffer = await fetch(loginBackgroundUrl).then((res) => res.arrayBuffer());

		return new HttpResponse(imageBuffer, {
			headers: {
				'Content-Length': imageBuffer.byteLength.toString(),
				'Content-Type': 'image/jpeg',
			},
		});
	}),
];
