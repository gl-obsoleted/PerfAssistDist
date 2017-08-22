""" @package gen_dist
<pre>
Usage:
    gen_dist.py stage [<component>]
    gen_dist.py version <major> <minor> <patch> [--postfix=<str>]

Options:
    -h --help                   Show this screen.
    --postfix=<str>             The postfix part of the version string, e.g. 'a' in 'v1.0.1a'
</pre>
"""

import logging
import sys
import shutil
import os

_join = os.path.join

PA_RootDir = os.path.dirname(os.path.abspath(__file__));

# add additional paths
sys.path.append(_join(PA_RootDir, "_External", "docopt"))
# log.info(sys.path)

import docopt

log = logging.getLogger(__name__)

PA_Components = [ 'PA_Common', 'PA_CoroutineTracker', 'PA_ResourceTracker' , 'PA_VarTracer']

PA_CopyConfig = dict(
    PA_Common=dict(src_dir="", dest_dir="PA_Common", ignore=shutil.ignore_patterns('.git', 'Docs*', 'README*')),
    PA_CoroutineTracker=dict(src_dir=_join("Assets", "PerfAssist", "CoroutineTracker"), dest_dir="CoroutineTracker", ignore=shutil.ignore_patterns()),
    PA_ResourceTracker=dict(src_dir=_join("Assets", "PerfAssist", "ResourceTracker"), dest_dir="ResourceTracker", ignore=shutil.ignore_patterns()),
    PA_VarTracer=dict(src_dir=_join("PA_VarTracer", "Assets", "PerfAssist","PAVarTracer"), dest_dir="PAVarTracer", ignore=shutil.ignore_patterns()),
)

PA_CoroutineTracker_PluginsDir = _join("Assets", "Plugins", "CoroutineTracker")

def getCompSrcDir(comp):
    return _join(PA_RootDir, "Components", comp)

def getCompDestDir(dirname):
    return _join(PA_RootDir, "PA_Staging", "Assets", "PerfAssist", dirname)

def perform_copy(src, dest, _ignore=None):
    if os.path.exists(dest):
        shutil.rmtree(dest)

    # log.info("src: \n  %s", src)
    # log.info("dest: \n  %s", dest)
    shutil.copytree(src, dest, ignore=_ignore)
    if os.path.isfile(dest + '.meta'):
        os.remove(dest + '.meta')
    if os.path.isfile(src + '.meta'):
        shutil.copyfile(src + '.meta', dest + '.meta')

def do_stage(args):
    log.info("staging begins.")
    filt = args['<component>'] or ''
    components = [comp for comp in PA_Components if filt.lower() in comp.lower() and comp in PA_CopyConfig]
    # log.info("components: \n  %s", components)
    for comp in components:
        log.info("staging: %s..", comp)
        cfg = PA_CopyConfig[comp]
        src = getCompSrcDir(comp)
        if cfg['src_dir'] != '':
            src = _join(src, cfg['src_dir'])
        dest = _join(getCompDestDir(cfg['dest_dir']))
        perform_copy(src, dest, _ignore=cfg['ignore'])

        # special case: copy 'Plugins' files for coroutine tracker
        if comp == 'PA_CoroutineTracker':
            perform_copy(
                _join(PA_RootDir, "Components", 'PA_CoroutineTracker', PA_CoroutineTracker_PluginsDir), 
                _join(PA_RootDir, "PA_Staging", PA_CoroutineTracker_PluginsDir))
    log.info("staging done.")


def do_package(args):
    pass


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO)

    args = docopt.docopt(__doc__)
    # log.info("args: \n  {}".format(args))

    PA_Commands = dict(stage=do_stage, package=do_package)
    for k,v in PA_Commands.iteritems():
        if k in args and args[k]:
            v(args)
            break
