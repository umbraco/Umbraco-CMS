import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUINumberRangeElement } from './property-editor-ui-number-range.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-number-range.element.js';

export default {
	title: 'Property Editor UIs/Number Range',
	component: 'umb-property-editor-ui-number-range',
	id: 'umb-property-editor-ui-number-range',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUINumberRangeElement> = () =>
	html`<umb-property-editor-ui-number-range></umb-property-editor-ui-number-range>`;
AAAOverview.storyName = 'Overview';
