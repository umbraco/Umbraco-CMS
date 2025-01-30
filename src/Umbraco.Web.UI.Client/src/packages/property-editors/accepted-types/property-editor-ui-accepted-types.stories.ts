import type { UmbPropertyEditorUIAcceptedTypesElement } from './property-editor-ui-accepted-types.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-accepted-types.element.js';

export default {
	title: 'Property Editor UIs/Accepted Types',
	component: 'umb-property-editor-ui-accepted-types',
	id: 'umb-property-editor-ui-accepted-types',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIAcceptedTypesElement> = () =>
	html`<umb-property-editor-ui-accepted-types></umb-property-editor-ui-accepted-types>`;
AAAOverview.storyName = 'Overview';
