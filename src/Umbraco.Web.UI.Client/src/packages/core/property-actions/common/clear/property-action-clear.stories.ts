import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyActionClearElement } from './property-action-clear.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-action-clear.element.js';

export default {
	title: 'Property Actions/Clear',
	component: 'umb-property-action-clear',
	id: 'umb-property-action-clear',
} as Meta;

export const AAAOverview: Story<UmbPropertyActionClearElement> = () =>
	html` <umb-property-action-clear></umb-property-action-clear>`;
AAAOverview.storyName = 'Overview';
