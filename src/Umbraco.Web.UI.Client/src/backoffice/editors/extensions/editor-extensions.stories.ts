import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbEditorExtensionsElement } from './editor-extensions.element';
import './editor-extensions.element';

import { data } from '../../../mocks/data/document-type.data';

export default {
	title: 'Editors/Extensions',
	component: 'umb-editor-extensions',
	id: 'umb-editor-extensions',
} as Meta;

export const AAAOverview: Story<UmbEditorExtensionsElement> = () =>
	html` <umb-editor-extensions></umb-editor-extensions>`;
AAAOverview.storyName = 'Overview';
