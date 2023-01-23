import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUIContentPickerElement } from './property-editor-ui-document-picker.element';
import { UmbModalService } from 'src/core/modal';
import './property-editor-ui-document-picker.element';
import '../../../components/backoffice-frame/backoffice-modal-container.element';

export default {
	title: 'Property Editor UIs/Content Picker',
	component: 'umb-property-editor-ui-document-picker',
	id: 'umb-property-editor-ui-document-picker',
	decorators: [
		(story) =>
			html`<umb-context-provider key="umbModalService" .value=${new UmbModalService()}>
				${story()}
				<umb-backoffice-modal-container></umb-backoffice-modal-container>
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIContentPickerElement> = () =>
	html` <umb-property-editor-ui-document-picker></umb-property-editor-ui-document-picker>`;
AAAOverview.storyName = 'Overview';
