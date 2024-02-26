import type { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyLayoutElement } from './property-layout.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-layout.element.js';

export default {
	title: 'Workspaces/Property Layout',
	component: 'umb-property-layout',
	id: 'umb-property-layout',
} as Meta;

export const AAAOverview: Story<UmbPropertyLayoutElement> = () =>
	html` <umb-property-layout label="Label" description="Description">
		<div slot="action-menu"><uui-button color="" look="placeholder">Action Menu</uui-button></div>

		<div slot="editor"><uui-button color="" look="placeholder">Editor</uui-button></div>
	</umb-property-layout>`;
AAAOverview.storyName = 'Overview';
