import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../context/document-workspace.context-token.js';
import { UmbDocumentSaveWorkspaceAction } from './save.action.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { BehaviorSubject, firstValueFrom, map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('umb-test-document-save-action-host')
class UmbTestDocumentSaveActionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const INVARIANT = UmbVariantId.CreateInvariant();
const ENGLISH = new UmbVariantId('en-US');
const DANISH = new UmbVariantId('da-DK');

class UmbTestDocumentWorkspaceContext {
	variants = new BehaviorSubject<Array<{ culture: string | null; segment: string | null }>>([]);
	variesByCulture = new BehaviorSubject<boolean | undefined>(undefined);
	isNew = new BehaviorSubject<boolean | undefined>(undefined);
	unique = new BehaviorSubject<string | undefined>('document-unique');
	writableVariants = new BehaviorSubject<Array<UmbVariantId>>([]);
	writableQueries = 0;

	constructor(private readonly hostElement: HTMLElement) {}

	getHostElement() {
		return this.hostElement;
	}

	getEntityType() {
		return UMB_DOCUMENT_ENTITY_TYPE;
	}

	isWritableVariant(variantId: UmbVariantId) {
		this.writableQueries++;
		return this.writableVariants.pipe(map((writable) => writable.some((x) => x.equal(variantId))));
	}

	setDocument(args: { variants: Array<UmbVariantId>; variesByCulture: boolean; isNew: boolean }) {
		this.variants.next(args.variants.map((variantId) => variantId.toObject()));
		this.variesByCulture.next(args.variesByCulture);
		this.isNew.next(args.isNew);
	}
}

describe('UmbDocumentSaveWorkspaceAction', () => {
	let providerHost: UmbTestDocumentSaveActionHostElement;
	let actionHost: UmbTestDocumentSaveActionHostElement;
	let context: UmbTestDocumentWorkspaceContext;
	let action: UmbDocumentSaveWorkspaceAction;

	async function isDisabled() {
		await waitUntil(() => context.writableQueries > 0, 'the action did not observe the writable variants');
		return firstValueFrom(action.isDisabled);
	}

	beforeEach(() => {
		providerHost = new UmbTestDocumentSaveActionHostElement();
		actionHost = new UmbTestDocumentSaveActionHostElement();
		providerHost.appendChild(actionHost);

		context = new UmbTestDocumentWorkspaceContext(providerHost);
		new UmbContextProviderController(providerHost, UMB_DOCUMENT_WORKSPACE_CONTEXT, context as never);
		action = new UmbDocumentSaveWorkspaceAction(actionHost, { meta: {} as never });

		document.body.appendChild(providerHost);
	});

	afterEach(() => {
		action.destroy();
		document.body.innerHTML = '';
	});

	describe('content that does not vary by culture', () => {
		it('is enabled for new content when its variant is writable', async () => {
			context.setDocument({ variants: [INVARIANT], variesByCulture: false, isNew: true });
			context.writableVariants.next([INVARIANT]);

			expect(await isDisabled()).to.be.false;
		});

		it('is disabled when its variant is not writable', async () => {
			context.setDocument({ variants: [INVARIANT], variesByCulture: false, isNew: false });

			expect(await isDisabled()).to.be.true;
		});
	});

	describe('content that varies by culture', () => {
		it('is enabled when a culture variant is writable', async () => {
			context.setDocument({ variants: [ENGLISH, DANISH], variesByCulture: true, isNew: true });
			context.writableVariants.next([DANISH]);

			expect(await isDisabled()).to.be.false;
		});

		it('is disabled when nothing is writable', async () => {
			context.setDocument({ variants: [ENGLISH, DANISH], variesByCulture: true, isNew: false });

			expect(await isDisabled()).to.be.true;
		});

		it('is disabled for new content when only the shared data is writable', async () => {
			context.setDocument({ variants: [ENGLISH, DANISH], variesByCulture: true, isNew: true });
			context.writableVariants.next([INVARIANT]);

			expect(await isDisabled()).to.be.true;
		});

		it('is enabled for existing content when only the shared data is writable', async () => {
			context.setDocument({ variants: [ENGLISH, DANISH], variesByCulture: true, isNew: false });
			context.writableVariants.next([INVARIANT]);

			expect(await isDisabled()).to.be.false;
		});

		it('is enabled once new content exists when only the shared data is writable', async () => {
			context.setDocument({ variants: [ENGLISH, DANISH], variesByCulture: true, isNew: true });
			context.writableVariants.next([INVARIANT]);
			expect(await isDisabled()).to.be.true;

			context.isNew.next(false);

			expect(await isDisabled()).to.be.false;
		});

		it('is enabled when only the shared data is writable and the culture variation resolves after the variants', async () => {
			context.variants.next([ENGLISH.toObject(), DANISH.toObject()]);
			context.isNew.next(false);
			context.writableVariants.next([INVARIANT]);
			expect(await isDisabled()).to.be.true;

			context.variesByCulture.next(true);

			expect(await isDisabled()).to.be.false;
		});
	});
});
