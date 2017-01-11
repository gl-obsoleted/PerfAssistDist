""" @package gen_dist
<pre>
Usage:
    gen_dist.py
    
Options:
    -h --help                   Show this screen.
</pre>
"""

    # gen_redist.py --dir_src=<dir> --dir_dest=<dir> --pkg_name=<name> --pkg_version=<version> [--redist_proc=<name>] [--is_debug] [--is_x64]
    # --dir_src=<dir>             The source folder of redist. 
    # --dir_dest=<dir>            The target folder of redist. 
    # --pkg_name=<name>           The name string.
    # --pkg_version=<version>     The version string. [default: v1.0]
    # --is_debug                  Whether it's the debug version
    # --is_x64                    Whether it's the x64 version


import logging
import sys
import shutil
import os

# add additional paths
sys.path.append(os.path.join(os.path.dirname(os.path.abspath(__file__)), "_External", "docopt"))
# print(sys.path)

import docopt

log = logging.getLogger(__name__)

# pkg_redist_procs = {}
# pkg_redist_procs[procs.libbitcoin.__name__] = procs.libbitcoin.redist_proc

# def cleanup(dest_dir):
#     if os.path.isdir(dest_dir):
#         log.info("Cleaning up dir '%s'.", dest_dir)
#         shutil.rmtree(dest_dir)
#     else:
#         log.info("Cleaning up skipped, dir '%s' doesn't exist.", dest_dir)

def main():
    logging.basicConfig(level=logging.INFO)

    # global preparing works: parsing args, configuring logging
    args = docopt.docopt(__doc__)
    
    # # validating parameters
    # src = args["--dir_src"]
    # if not os.path.isdir(src):
    #     log.error("Source directory (%s) not found (or not a valid directory).", src)
    #     return -1
    # dest = args["--dir_dest"]
    # if not os.path.isdir(dest):
    #     log.error("Target directory (%s) not found (or not a valid directory).", dest)
    #     return -1

    # # preparing redist folder
    # redist_folder = "{}-{}".format(args["--pkg_name"], args["--pkg_version"] )
    # if args["--is_debug"]:
    #     redist_folder += "-d"
    # if args["--is_x64"]:
    #     redist_folder += "-x64"

    # redist_path = os.path.join(dest, redist_folder)
    # cleanup(redist_path)

    # # performing the redistribution process
    # try:
    #     if args["--redist_proc"]:
    #         proc = pkg_redist_procs[args["--redist_proc"]]
    #         if proc:
    #             log.info("Triggering registered redist process. ('%s')", args["--redist_proc"])
    #             proc(src, redist_path)
    #         else:
    #             log.error("Proc (%s) not found (or not registered correctly).", args["--redist_proc"])
    #     else:
    #         # copying files
    #         log.info("Copying dir '%s' to '%s'.", src, redist_path)
    #         shutil.copytree(src, redist_path)   # propergate the exception freely
    # except Exception as e:
    #     cleanup(redist_path)
    #     raise e

    return 0
 
if __name__ == '__main__':
    ret = main()
    sys.exit(ret)
