import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyElement } from './entity-property.element';
import './entity-property.element';

export default {
	title: 'Components/Entity Property',
	component: 'umb-property',
	id: 'umb-property',
} as Meta;

export const AAAOverview: Story<UmbPropertyElement> = () =>
	html` <umb-property
		label="Property"
		description="Description"
		alias="textProperty"
		property-editor-ui-alias="Umb.PropertyEditorUI.TextBox"
		.value="${'Hello'}"></umb-property>`;
AAAOverview.storyName = 'Overview';
