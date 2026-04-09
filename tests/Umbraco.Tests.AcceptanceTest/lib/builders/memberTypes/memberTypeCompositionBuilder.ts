import {MemberTypeBuilder} from './memberTypeBuilder';
import {buildComposition} from '../../helpers/BuilderUtils';

export class MemberTypeCompositionBuilder {
  parentBuilder: MemberTypeBuilder;
  memberTypeId: string
  compositionType: string;

  constructor(parentBuilder: MemberTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withMemberTypeId(memberTypeId: string) {
    this.memberTypeId = memberTypeId;
    return this;
  }

  withCompositionType(compositionType: string) {
    this.compositionType = compositionType;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return buildComposition('memberType', this.memberTypeId, this.compositionType);
  }
}