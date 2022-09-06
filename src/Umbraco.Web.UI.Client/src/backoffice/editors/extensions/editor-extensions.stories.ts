import './editor-extensions.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbEditorExtensionsElement } from './editor-extensions.element';

export default {
	title: 'Editors/Extensions',
	component: 'umb-editor-extensions',
	id: 'umb-editor-extensions',
} as Meta;

export const AAAOverview: Story<UmbEditorExtensionsElement> = () =>
	html` <umb-editor-extensions></umb-editor-extensions>`;
AAAOverview.storyName = 'Overview';
