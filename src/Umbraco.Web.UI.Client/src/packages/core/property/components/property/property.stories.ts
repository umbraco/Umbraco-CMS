import type { UmbPropertyElement } from './property.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property.element.js';

export default {
	title: 'Components/Property',
	component: 'umb-property',
	id: 'umb-property',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyElement> = () =>
	html` <umb-property
		label="Property"
		description="Description"
		alias="textProperty"
		property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
		.value="${'Hello'}"></umb-property>`;
AAAOverview.storyName = 'Overview';
