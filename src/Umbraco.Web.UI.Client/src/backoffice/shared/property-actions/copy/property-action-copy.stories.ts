import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyActionCopyElement } from './property-action-copy.element';
import './property-action-copy.element';

export default {
	title: 'Property Actions/Copy',
	component: 'umb-property-action-copy',
	id: 'umb-property-action-copy',
} as Meta;

export const AAAOverview: Story<UmbPropertyActionCopyElement> = () =>
	html` <umb-property-action-copy></umb-property-action-copy>`;
AAAOverview.storyName = 'Overview';
