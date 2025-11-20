import './footer-layout.element.js';

import type { UmbFooterLayoutElement } from './footer-layout.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Generic Components/Footer Layout',
	component: 'umb-footer-layout',
	id: 'umb-footer-layout',
} as Meta;

export const Docs: StoryFn<UmbFooterLayoutElement> = () =>
	html` <umb-body-layout>
		<div slot="footer-info">
			<uui-button color="" look="placeholder">Footer slot</uui-button
			><uui-button color="" look="placeholder">Actions slot</uui-button>
		</div>
	</umb-body-layout>`;
