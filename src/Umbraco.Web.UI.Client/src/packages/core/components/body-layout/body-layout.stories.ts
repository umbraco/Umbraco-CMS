import type { UmbBodyLayoutElement } from './body-layout.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './body-layout.element.js';

const meta: Meta<UmbBodyLayoutElement> = {
	title: 'Generic Components/Body Layout',
	component: 'umb-body-layout',
};

export default meta;
type Story = StoryObj<UmbBodyLayoutElement>;

export const Docs: Story = {
	render: () =>
		html` <umb-body-layout>
			<div slot="header"><uui-button color="" look="placeholder">Header slot</uui-button></div>
			<uui-button color="" look="placeholder">Main slot</uui-button>
			<div slot="footer-info"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
		</umb-body-layout>`,
};
