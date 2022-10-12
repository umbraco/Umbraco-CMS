import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { internalManifests } from '../../../../temp-internal-manifests';
import type { ManifestSection } from '../../../../core/models';
import { UmbSectionContext } from '../../section.context';
import type { UmbSectionDashboardsElement } from './section-dashboards.element';
import './section-dashboards.element';

const contentSectionManifest = internalManifests.find((m) => m.alias === 'Umb.Section.Content') as ManifestSection;

export default {
	title: 'Sections/Shared/Section Dashboards',
	component: 'umb-section-dashboards',
	id: 'umb-section-dashboards',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbSectionContext" .value=${new UmbSectionContext(contentSectionManifest)}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbSectionDashboardsElement> = () =>
	html` <umb-section-dashboards></umb-section-dashboards> `;
AAAOverview.storyName = 'Overview';
