import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbWorkspacePropertyElement } from './workspace-property.element';
import './workspace-property.element';

export default {
	title: 'Components/Entity Property',
	component: 'umb-workspace-property',
	id: 'umb-workspace-property',
} as Meta;

export const AAAOverview: Story<UmbWorkspacePropertyElement> = () =>
	html` <umb-workspace-property
		label="Property"
		description="Description"
		alias="textProperty"
		property-editor-ui-alias="Umb.PropertyEditorUI.TextBox"
		.value="${'Hello'}"></umb-workspace-property>`;
AAAOverview.storyName = 'Overview';
