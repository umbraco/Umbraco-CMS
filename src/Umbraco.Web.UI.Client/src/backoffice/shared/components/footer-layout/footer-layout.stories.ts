import './footer-layout.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbFooterLayout } from './footer-layout.element';

export default {
	title: 'Workspaces/Shared/Footer Layout',
	component: 'umb-footer-layout',
	id: 'umb-footer-layout',
} as Meta;

export const AAAOverview: Story<UmbFooterLayout> = () => html` <umb-body-layout>
	<div slot="footer">
		<uui-button color="" look="placeholder">Footer slot</uui-button
		><uui-button color="" look="placeholder">Actions slot</uui-button>
	</div>
</umb-body-layout>`;
AAAOverview.storyName = 'Overview';
