import './ref-property-editor-ui.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbRefPropertyEditorUIElement } from './ref-property-editor-ui.element';

export default {
	title: 'Components/Ref Property Editor UI',
	component: 'umb-ref-property-editor-ui',
	id: 'umb-ref-property-editor-ui',
} as Meta;

export const AAAOverview: Story<UmbRefPropertyEditorUIElement> = () =>
	html` <umb-ref-property-editor-ui
		name="Custom Property Editor UI"
		alias="Umb.PropertyEditor.CustomUI"></umb-ref-property-editor-ui>`;
AAAOverview.storyName = 'Overview';
