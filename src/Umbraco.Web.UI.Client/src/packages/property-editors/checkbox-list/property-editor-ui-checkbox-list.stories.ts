import type { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-checkbox-list.element.js';

export default {
	title: 'Property Editor UIs/Checkbox List',
	component: 'umb-property-editor-ui-checkbox-list',
	id: 'umb-property-editor-ui-checkbox-list',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUICheckboxListElement> = () =>
	html`<umb-property-editor-ui-checkbox-list></umb-property-editor-ui-checkbox-list>`;
AAAOverview.storyName = 'Overview';
