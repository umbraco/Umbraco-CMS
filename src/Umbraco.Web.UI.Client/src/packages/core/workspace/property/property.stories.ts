import { Meta, Story } from '@storybook/web-components';
import type { UmbWorkspacePropertyElement } from './property.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property.element.js';

export default {
	title: 'Components/Property',
	component: 'umb-property',
	id: 'umb-property',
} as Meta;

export const AAAOverview: Story<UmbWorkspacePropertyElement> = () =>
	html` <umb-workspace-property
		label="Property"
		description="Description"
		alias="textProperty"
		property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
		.value="${'Hello'}"></umb-workspace-property>`;
AAAOverview.storyName = 'Overview';
