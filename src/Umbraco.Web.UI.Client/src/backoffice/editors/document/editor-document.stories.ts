import './editor-document.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../mocks/data/node.data';

import type { UmbEditorDocumentElement } from './editor-document.element';

export default {
	title: 'Editors/Document',
	component: 'umb-editor-document',
	id: 'umb-editor-document',
} as Meta;

const documentNodes = data.filter((node) => node.type === 'document');

export const AAAOverview: Story<UmbEditorDocumentElement> = () =>
	html` <umb-editor-document id="${documentNodes[0].key}"></umb-editor-document>`;
AAAOverview.storyName = 'Overview';
