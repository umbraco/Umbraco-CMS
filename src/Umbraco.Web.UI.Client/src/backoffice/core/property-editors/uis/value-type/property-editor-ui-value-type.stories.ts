import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIValueTypeElement } from './property-editor-ui-value-type.element';
import './property-editor-ui-value-type.element';

export default {
	title: 'Property Editor UIs/Value Type',
	component: 'umb-property-editor-ui-value-type',
	id: 'umb-property-editor-ui-value-type',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIValueTypeElement> = () =>
	html`<umb-property-editor-ui-value-type></umb-property-editor-ui-value-type>`;
AAAOverview.storyName = 'Overview';
