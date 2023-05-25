import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyActionCopyElement } from './property-action-copy.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-action-copy.element.js';

export default {
	title: 'Property Actions/Copy',
	component: 'umb-property-action-copy',
	id: 'umb-property-action-copy',
} as Meta;

export const AAAOverview: Story<UmbPropertyActionCopyElement> = () =>
	html` <umb-property-action-copy></umb-property-action-copy>`;
AAAOverview.storyName = 'Overview';
