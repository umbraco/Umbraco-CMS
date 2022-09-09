import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorContentPickerElement } from './property-editor-content-picker.element';
import './property-editor-content-picker.element';
import { UmbModalService } from '../../../core/services/modal';
import '../../../backoffice/components/backoffice-modal-container.element';

export default {
	title: 'Property Editors/Content Picker',
	component: 'umb-property-editor-content-picker',
	id: 'umb-property-editor-content-picker',
	decorators: [
		(story) =>
			html`<umb-context-provider key="umbModalService" .value=${new UmbModalService()}>
				${story()}
				<umb-backoffice-modal-container></umb-backoffice-modal-container>
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorContentPickerElement> = () =>
	html` <umb-property-editor-content-picker></umb-property-editor-content-picker>`;
AAAOverview.storyName = 'Overview';
