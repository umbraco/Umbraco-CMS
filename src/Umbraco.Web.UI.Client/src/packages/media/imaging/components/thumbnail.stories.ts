import './thumbnail.element.js';
import { UmbImagingCropMode } from '../types.js';
import type { UmbThumbnailElement } from './thumbnail.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html, ref } from '@umbraco-cms/backoffice/external/lit';

// A transparent SVG (magenta disc on a transparent canvas) so the checkerboard shows in the corners.
const SAMPLE_IMAGE =
	"data:image/svg+xml,%3Csvg%20xmlns='http://www.w3.org/2000/svg'%20width='160'%20height='160'%3E%3Ccircle%20cx='80'%20cy='80'%20r='70'%20fill='%23e3066e'/%3E%3C/svg%3E";

// The component resolves its image URL from the server via UmbImagingRepository. Storybook has no
// imaging backend, so this story-only helper sets the resolved URL directly to demonstrate rendering.
// The stable callback identity ensures the ref only runs on mount, not on every update.
const withSampleImage = (el?: Element) => {
	const thumbnail = el as (UmbThumbnailElement & { _thumbnailUrl: string }) | undefined;
	if (thumbnail && thumbnail._thumbnailUrl !== SAMPLE_IMAGE) {
		thumbnail._thumbnailUrl = SAMPLE_IMAGE;
		thumbnail.requestUpdate();
	}
};

const meta: Meta<UmbThumbnailElement> = {
	title: 'Entity/Media/Components/Thumbnail',
	component: 'umb-thumbnail',
	args: {
		width: 300,
		height: 300,
		mode: UmbImagingCropMode.MIN,
		alt: 'Sample image',
		icon: 'icon-picture',
		loading: 'eager',
	},
	render: (args) => html`
		<div style="width: 200px; height: 200px;">
			<umb-thumbnail
				${ref(withSampleImage)}
				.width=${args.width}
				.height=${args.height}
				.mode=${args.mode}
				.alt=${args.alt}
				.icon=${args.icon}
				.loading=${args.loading}></umb-thumbnail>
		</div>
	`,
};

export default meta;
type Story = StoryObj<UmbThumbnailElement>;

/**
 * The default behaviour: transparent areas of the image are shown over a checkerboard pattern, which
 * signals transparency while managing media.
 */
export const Default: Story = {};

/**
 * Setting `--umb-thumbnail-background: none` removes the checkerboard so the image sits on a transparent
 * background — use this when the thumbnail represents final content, e.g. in a block grid. Here it is
 * shown over a solid colour to make the transparency visible.
 */
export const TransparentBackground: Story = {
	render: (args) => html`
		<div style="width: 200px; height: 200px; background: #2152a3; --umb-thumbnail-background: none;">
			<umb-thumbnail
				${ref(withSampleImage)}
				.width=${args.width}
				.height=${args.height}
				.mode=${args.mode}
				.alt=${args.alt}
				.icon=${args.icon}
				.loading=${args.loading}></umb-thumbnail>
		</div>
	`,
};

/**
 * When the media item has no image (or no `unique` is provided), the fallback `icon` is rendered.
 */
export const FallbackIcon: Story = {
	render: (args) => html`
		<div style="width: 200px; height: 200px;">
			<umb-thumbnail .icon=${args.icon} .alt=${args.alt}></umb-thumbnail>
		</div>
	`,
};
