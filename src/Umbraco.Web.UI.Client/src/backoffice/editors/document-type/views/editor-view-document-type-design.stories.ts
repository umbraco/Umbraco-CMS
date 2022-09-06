import './editor-view-document-type-design.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../mocks/data/document-type.data';
import { UmbDocumentTypeContext } from '../document-type.context';

import type { UmbEditorViewDocumentTypeDesignElement } from './editor-view-document-type-design.element';

export default {
	title: 'Editors/Document Type/Views/Design',
	component: 'umb-editor-view-document-type-design',
	id: 'umb-editor-view-document-type-design',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbDocumentTypeContext" .value=${new UmbDocumentTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewDocumentTypeDesignElement> = () =>
	html` <umb-editor-view-document-type-design></umb-editor-view-document-type-design>`;
AAAOverview.storyName = 'Overview';
