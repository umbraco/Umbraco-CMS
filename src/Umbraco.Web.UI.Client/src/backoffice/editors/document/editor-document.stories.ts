import './editor-document.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data as documentNodes } from '../../../core/mocks/data/document.data';

import type { UmbEditorDocumentElement } from './editor-document.element';

export default {
	title: 'Editors/Document',
	component: 'umb-editor-document',
	id: 'umb-editor-document',
} as Meta;

export const AAAOverview: Story<UmbEditorDocumentElement> = () =>
	html` <umb-editor-document id="${documentNodes[0].key}"></umb-editor-document>`;
AAAOverview.storyName = 'Overview';
